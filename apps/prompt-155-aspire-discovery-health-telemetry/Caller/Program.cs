using Microsoft.Extensions.ServiceDiscovery;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceDiscovery();
builder.Services.AddHttpClient("catalog", client => client.BaseAddress = new Uri("http://api"))
    .AddServiceDiscovery();
builder.Services.AddHttpClient("unknown", client => { client.BaseAddress = new Uri("http://unconfigured.invalid"); client.Timeout = TimeSpan.FromSeconds(2); })
    .AddServiceDiscovery();
var app = builder.Build();
app.MapGet("/health", () => Results.Ok());
app.MapGet("/discover", async (IHttpClientFactory clients) => await clients.CreateClient("catalog").GetStringAsync("/course"));
app.MapGet("/unknown", async (IHttpClientFactory clients) =>
{
    try { await clients.CreateClient("unknown").GetStringAsync("/course"); return Results.Ok(); }
    catch (HttpRequestException) { return Results.StatusCode(503); }
    catch (TaskCanceledException) { return Results.StatusCode(503); }
});
app.Run();
