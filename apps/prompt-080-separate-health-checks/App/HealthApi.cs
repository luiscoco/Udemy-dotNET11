using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
public sealed class HealthApi(HealthState state) : IAsyncDisposable
{
    private WebApplication app = null!;
    public async Task StartAsync(bool coupleLiveness)
    {
        var builder = WebApplication.CreateBuilder(); builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(state);
        builder.Services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(2) });
        builder.Services.AddHealthChecks()
            .AddCheck("process", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("startup", () => state.Started ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Starting"), tags: ["ready"])
            .AddCheck<CatalogHealth>("catalog", tags: ["ready", "dependency"])
            .AddCheck("cache", () => state.CacheWarming ? HealthCheckResult.Degraded("Warming") : HealthCheckResult.Healthy(),
                tags: ["ready", "dependency", "optional"]);
        app = builder.Build();
        app.MapGet("/dependency", () =>
        {
            Interlocked.Increment(ref state.DependencyCalls);
            return Results.StatusCode(state.DependencyAvailable ? 200 : 503);
        });
        app.MapHealthChecks("/health/live", Options(registration => coupleLiveness || registration.Tags.Contains("live")));
        app.MapHealthChecks("/health/ready", Options(registration => registration.Tags.Contains("ready")));
        app.MapHealthChecks("/health/dependencies", Options(registration => registration.Tags.Contains("dependency")));
        app.MapHealthChecks("/health/optional", Options(registration => registration.Tags.Contains("optional")));
        await app.StartAsync(); state.Address = app.Urls.Single();
    }
    private static HealthCheckOptions Options(Func<HealthCheckRegistration, bool> predicate) => new()
    {
        Predicate = predicate,
        ResultStatusCodes = new Dictionary<HealthStatus, int>
            { [HealthStatus.Healthy] = 200, [HealthStatus.Degraded] = 200, [HealthStatus.Unhealthy] = 503 },
        ResponseWriter = async (context, report) => await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new { name = entry.Key, status = entry.Value.Status.ToString() })
        })
    };
    public async ValueTask DisposeAsync() { await app.StopAsync(); await app.DisposeAsync(); }
}
