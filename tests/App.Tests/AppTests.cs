using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace App.Tests;

public sealed class AppTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthCheckReportsHealthy()
    {
        var response = await factory.CreateClient().GetAsync("/healthz", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HomePageIsServed()
    {
        var response = await factory.CreateClient().GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Your ASP.NET Core app is running.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StatusReportsTheRuntime()
    {
        var status = await factory.CreateClient().GetFromJsonAsync<ApplicationStatus>("/api/status", TestContext.Current.CancellationToken);

        Assert.NotNull(status);
        Assert.Equal("ok", status.Status);
        Assert.StartsWith(".NET 10", status.Framework);
    }

    [Fact]
    public async Task UnknownPathIsNotFound()
    {
        var response = await factory.CreateClient().GetAsync("/missing", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("example.com", HttpStatusCode.OK)]
    [InlineData("www.example.com", HttpStatusCode.OK)]
    [InlineData("app", HttpStatusCode.OK)]
    [InlineData("localhost", HttpStatusCode.OK)]
    [InlineData("attacker.test", HttpStatusCode.BadRequest)]
    public async Task OnlyWodbyHostsAreServed(string host, HttpStatusCode expected)
    {
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("WODBY_HOSTS", """["example.com","www.example.com"]""");
            builder.UseSetting("WODBY_APP_SERVICE_NAME", "app");
        }).CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/healthz");
        request.Headers.Host = host;

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(expected, response.StatusCode);
    }

    private sealed record ApplicationStatus(string Status, string Framework, string Environment);
}
