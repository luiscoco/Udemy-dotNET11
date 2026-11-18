using System.Text.Json;
var state = new HealthState();
await using var api = new HealthApi(state);
await api.StartAsync(args.FirstOrDefault() == "coupled");
using var client = new HttpClient();
async Task<(int Code, string Status)> ReadAsync(string path)
{
    using var response = await client.GetAsync(state.Address + path);
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    string status = json.RootElement.GetProperty("status").GetString()!;
    Console.WriteLine($"{path}: HTTP {(int)response.StatusCode} / {status}");
    return ((int)response.StatusCode, status);
}
if (args.FirstOrDefault() == "coupled")
{
    state.Started = true; state.DependencyAvailable = false;
    Check.That((await ReadAsync("/health/live")).Code == 200, "a dependency failure must not become a process-liveness failure under this policy");
    return;
}
int before = state.DependencyCalls;
Check.That(await ReadAsync("/health/live") == (200, "Healthy") && state.DependencyCalls == before,
    "liveness checks only the process and makes no dependency request");
Check.That((await ReadAsync("/health/ready")).Code == 503, "readiness stays unavailable until startup is complete");
state.Started = true; state.CacheWarming = true;
Check.That(await ReadAsync("/health/dependencies") == (200, "Degraded"), "optional cache warmup is an explicit degraded-success policy");
Check.That(await ReadAsync("/health/optional") == (200, "Degraded"), "the optional-dependency route exposes its own state");
Check.That(await ReadAsync("/health/ready") == (200, "Degraded"), "the ready policy allows the selected degraded condition");
state.DependencyAvailable = false;
Check.That((await ReadAsync("/health/ready")).Code == 503 && (await ReadAsync("/health/dependencies")).Code == 503,
    "a failed required HTTP dependency removes readiness");
Check.That((await ReadAsync("/health/live")).Code == 200, "the process remains live during a dependency outage");
state.DependencyAvailable = true; state.CacheWarming = false;
Check.That(await ReadAsync("/health/ready") == (200, "Healthy"), "readiness recovers after the required and optional checks recover");
