using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
public sealed class CatalogServer : IAsyncDisposable
{
    private WebApplication app = null!;
    private int active, peak;
    public int Peak => peak;
    public string Address { get; private set; } = "";
    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        app = builder.Build();
        app.MapGet("/api/items/{id:int}", async (int id, CancellationToken token) =>
        {
            if (id == 0) return Results.NotFound();
            int now = Interlocked.Increment(ref active), seen;
            do { seen = peak; if (now <= seen) break; }
            while (Interlocked.CompareExchange(ref peak, now, seen) != seen);
            try { await Task.Delay(30, token); return Results.Ok(new CatalogItem(id, $"item-{id}")); }
            finally { Interlocked.Decrement(ref active); }
        });
        app.MapGet("/api/slow", async (CancellationToken token) => { await Task.Delay(10_000, token); return "late"; });
        await app.StartAsync();
        Address = app.Urls.Single();
    }
    public async ValueTask DisposeAsync() { await app.StopAsync(); await app.DisposeAsync(); }
}
