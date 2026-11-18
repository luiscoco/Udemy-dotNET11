using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

bool broken = args.Contains("problem");
Directory.CreateDirectory("artifacts");
long observed = 0;
var spans = new List<string>();
using var meter = new MeterListener
{
    InstrumentPublished = (i, l) =>
    {
        if (i.Meter.Name == "Course.Operations")
            l.EnableMeasurementEvents(i);
    }
};
meter.SetMeasurementEventCallback<long>((i, v, t, s) => Interlocked.Add(ref observed, v));
meter.Start();
using var traces = new ActivityListener
{
    ShouldListenTo = s => s.Name == "Course.Operations",
    Sample = (ref ActivityCreationOptions<ActivityContext> o) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = a =>
    {
        lock (spans)
            spans.Add(a.OperationName + ":" + a.TraceId);
    }
};
ActivitySource.AddActivityListener(traces);
using var authority = new Authority();
using var state = new Operations
{
    Broken = broken
};
var b = ApiSession.Builder();
authority.Configure(b.Services);
b.Services.AddMemoryCache();
b.Services.AddSignalR();
b.Services.AddSingleton(state);
b.Services.AddHostedService<OperationsWorker>();
var app = b.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<OperationsHub>("/hub").RequireAuthorization();
app.MapGet("/", () => Results.Content(File.ReadAllText("dashboard.html"), "text/html"));
app.MapGet("/signalr.min.js", () => Results.File(Path.GetFullPath("assets/signalr.min.js"), "text/javascript"));
app.MapGet("/snapshot", (ClaimsPrincipal actor, IMemoryCache cache) =>
{
    string tenant = actor.FindFirstValue("tenant")!;
    return Results.Ok(new { value = cache.GetOrCreate(tenant, e =>
    {
        e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return state.Counts.GetValueOrDefault(tenant);
    }) });
}).RequireAuthorization();
app.MapPost("/jobs", async (ClaimsPrincipal actor) =>
{
    using var activity = state.Traces.StartActivity("enqueue-job", ActivityKind.Producer);
    await state.Queue.Writer.WriteAsync(new(actor.FindFirstValue("tenant")!, activity?.Context ?? default));
    return Results.Accepted();
}).RequireAuthorization("write");
await using var api = await ApiSession.Start(app);
Check.That((int)(await api.Client.GetAsync("/snapshot")).StatusCode == 401, "dashboard snapshot requires authentication");
using var browser = Harness.Start("node", new[] { "browser-test.cjs", api.Client.BaseAddress!.ToString(), broken ? "problem" : "verify" }, new Dictionary<string, string> { { "COURSE_DASHBOARD_TOKEN", authority.Issue() } });
var stdout = browser.StandardOutput.ReadToEndAsync();
var stderr = browser.StandardError.ReadToEndAsync();
await browser.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(80));
string output = await stdout;
Check.That(browser.ExitCode == 0, "actual Chrome dashboard completes its workflow", await stderr);
Console.WriteLine(output);
using var doc = JsonDocument.Parse(output);
var result = doc.RootElement;
int live = result.GetProperty("live").GetInt32(), snapshot = result.GetProperty("snapshot").GetInt32();
Console.WriteLine($"OPS actual SignalR live={live} cachedSnapshot={snapshot} completed={state.Completed} measurements={observed}");
Check.That(live == 1 && snapshot == 1, "cache invalidation keeps HTTP snapshot consistent with SignalR", "CAPSTONE operations cached snapshot failed");
Check.That(state.Completed == 1 && observed == 1 && spans.Count == 2 && spans.Select(s => s.Split(':')[1]).Distinct().Count() == 1, "background processing emits actual correlated producer/consumer spans and a measured completion");
Harness.Save("capstone-evidence.json", new { passed = true, actualSignalR = true, actualChrome = true, backgroundJobs = state.Completed, live, snapshot, observedMeasurements = observed, spans, browser = result.Clone() });
