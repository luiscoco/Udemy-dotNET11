using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
public sealed class Peer : IAsyncDisposable
{
    private WebApplication app = null!;
    public int FlakyCalls, FailingCalls;
    public TaskCompletionSource SlowExited { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string Address { get; private set; } = "";
    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder(); builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); app = builder.Build();
        app.MapGet("/flaky", () => Interlocked.Increment(ref FlakyCalls) <= 2 ? Results.StatusCode(503) : Results.Ok());
        app.MapGet("/failing", () => { Interlocked.Increment(ref FailingCalls); return Results.StatusCode(503); });
        app.MapGet("/slow", async (CancellationToken token) =>
        {
            try { await Task.Delay(10_000, token); return Results.Ok(); }
            finally { SlowExited.TrySetResult(); }
        });
        await app.StartAsync(); Address = app.Urls.Single();
    }
    public async ValueTask DisposeAsync() { await app.StopAsync(); await app.DisposeAsync(); }
}
