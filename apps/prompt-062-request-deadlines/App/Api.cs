using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
public sealed class Api : IAsyncDisposable
{
    public ConcurrentQueue<string> Trace { get; } = new();
    public TaskCompletionSource PeerEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource PeerExited { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly HttpClient peer = new();
    private WebApplication application = null!;
    public string Address { get; private set; } = "";
    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        application = builder.Build();
        var service = new RequestService(peer, Trace);
        application.MapGet("/peer/{mode}", async (string mode, CancellationToken token) =>
        {
            if (mode == "slow")
            {
                PeerEntered.TrySetResult();
                try { await Task.Delay(10_000, token); }
                finally { PeerExited.TrySetResult(); }
            }
            return "peer";
        });
        application.MapGet("/api/{mode}", async (string mode, HttpContext context) =>
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            if (mode is "database" or "outbound") deadline.CancelAfter(TimeSpan.FromMilliseconds(100));
            if (mode == "ignored") deadline.Cancel();
            try
            {
                var token = mode == "ignored" ? CancellationToken.None : deadline.Token;
                string result = await service.ExecuteAsync(mode, Address, token);
                return Results.Ok(result);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                Trace.Enqueue("api: client disconnected; no timeout response attempted");
                throw;
            }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested)
            {
                Trace.Enqueue($"api: cancellation observed for {mode}");
                return Results.StatusCode(408);
            }
        });
        await application.StartAsync();
        Address = application.Urls.Single();
    }
    public async ValueTask DisposeAsync()
    {
        await application.StopAsync();
        await application.DisposeAsync();
        peer.Dispose();
    }
}
