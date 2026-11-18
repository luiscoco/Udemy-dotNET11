using Npgsql;
using StackExchange.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDataSource("coursedb");
builder.AddRedisClient("cache");
builder.Services.AddSingleton<Readiness>();
builder.Services.AddHealthChecks().AddCheck<Readiness>("application-readiness");
builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing
    .AddAspNetCoreInstrumentation().AddProcessor(new TraceCounter()));
var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/alive", () => Results.Ok("running"));
app.MapPost("/ready", (Readiness readiness) => { readiness.Ready = true; return Results.Ok(); });
app.MapGet("/telemetry", () => TraceCounter.Count.ToString());
app.MapGet("/course", async (NpgsqlDataSource database, IConnectionMultiplexer cache) =>
{
    try
    {
        await using var command = database.CreateCommand("SELECT title FROM course_demo WHERE id=11");
        var title = await command.ExecuteScalarAsync();
        var cached = await cache.GetDatabase().StringGetAsync("course:11");
        return title is not null && cached.HasValue ? Results.Ok(new {title, cache=cached.ToString()}) : Results.StatusCode(503);
    }
    catch (PostgresException exception) when (exception.SqlState == "42P01") { return Results.StatusCode(503); }
});
app.Run();
public sealed class Readiness : IHealthCheck
{
    public bool Ready { get; set; } = false;
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
        => Task.FromResult(Ready ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Initialization pending"));
}
public sealed class TraceCounter : BaseProcessor<Activity>
{
    private static int count;
    public static int Count => Volatile.Read(ref count);
    public override void OnEnd(Activity activity) => Interlocked.Increment(ref count);
}
