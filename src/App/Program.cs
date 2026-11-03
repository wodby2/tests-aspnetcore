using System.Runtime.InteropServices;
using App;
using Microsoft.AspNetCore.HostFiltering;

var builder = WebApplication.CreateBuilder(args);

// Answer only for the environment's hosts once Wodby supplies them; without WODBY_HOSTS the
// AllowedHosts setting applies, as in any ASP.NET Core application.
builder.Services.AddOptions<HostFilteringOptions>()
    .Configure<IConfiguration>((options, configuration) =>
    {
        var hosts = WodbyHosts.AllowedHosts(configuration);
        if (hosts.Count > 0)
        {
            options.AllowedHosts = hosts;
        }
    });

// The health check covers this process only, so an optional linked service that is down
// does not take the application out of rotation.
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/status", (IWebHostEnvironment environment) => new ApplicationStatus(
    "ok",
    RuntimeInformation.FrameworkDescription,
    environment.EnvironmentName));

app.MapHealthChecks("/healthz");

app.Run();

/// <summary>The JSON body of <c>/api/status</c>.</summary>
internal sealed record ApplicationStatus(string Status, string Framework, string Environment);
