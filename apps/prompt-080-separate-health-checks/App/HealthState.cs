using Microsoft.Extensions.Diagnostics.HealthChecks;
public sealed class HealthState
{
    public volatile bool Started, DependencyAvailable = true, CacheWarming;
    public string Address { get; set; } = "";
    public int DependencyCalls;
}
public sealed class CatalogHealth(HttpClient client, HealthState state) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
    {
        using var response = await client.GetAsync(state.Address + "/dependency", token);
        return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Catalog unavailable");
    }
}
