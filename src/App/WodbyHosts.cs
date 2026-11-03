using System.Text.Json;

namespace App;

/// <summary>
/// Host names the application answers for on Wodby.
/// </summary>
public static class WodbyHosts
{
    /// <summary>
    /// Returns the environment's hosts from <c>WODBY_HOSTS</c>, a JSON array, together with
    /// <c>WODBY_APP_SERVICE_NAME</c>, the name other services and deployment checks use, and
    /// <c>localhost</c>. Explicit <c>AllowedHosts</c> entries are kept. Returns an empty list when
    /// <c>WODBY_HOSTS</c> is unset or empty, leaving host filtering to <c>AllowedHosts</c>.
    /// </summary>
    public static IList<string> AllowedHosts(IConfiguration configuration)
    {
        var value = configuration["WODBY_HOSTS"];
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var hosts = JsonSerializer.Deserialize<string[]>(value)
            ?? throw new InvalidOperationException("WODBY_HOSTS must be a JSON array of host names.");
        if (hosts.Length == 0)
        {
            return [];
        }

        var allowed = new List<string>(hosts) { "localhost" };
        if (configuration["WODBY_APP_SERVICE_NAME"] is { Length: > 0 } serviceName)
        {
            allowed.Add(serviceName);
        }

        var configured = configuration["AllowedHosts"]?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        allowed.AddRange(configured.Where(host => host != "*"));

        return allowed.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
