using Microsoft.Extensions.AI;
using System.Text.Json;
using App.Components;
if (File.Exists("../browser-driver.cjs")) Directory.SetCurrentDirectory("..");
bool buffered = args.Contains("problem");
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseStaticWebAssets();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IChatClient>(_ => AiRuntime.Create());
await using var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/favicon.ico", () => Results.NoContent());
app.MapRazorComponents<App.Components.App>().AddInteractiveServerRenderMode();
app.MapGet("/api/chat", async (HttpContext context, IChatClient chat) =>
{
    context.Response.ContentType = "application/x-ndjson";
    string pending = "";
    await foreach (var update in chat.GetStreamingResponseAsync(
        "Explain dependency injection in three sentences.", AiRuntime.Options(85), context.RequestAborted))
    {
        if (string.IsNullOrEmpty(update.Text)) continue;
        if (buffered) { pending += update.Text; continue; }
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { text = update.Text }) + "\n");
        await context.Response.Body.FlushAsync(context.RequestAborted);
    }
    if (buffered)
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { text = pending }) + "\n");
});
await app.StartAsync();
if (args.Contains("serve"))
{
    Console.WriteLine("Blazor lesson: " + app.Urls.Single());
    await app.WaitForShutdownAsync();
    return;
}
try
{
    string address = app.Urls.Single();
    using (var warmup = new HttpClient())
    {
        string html = await warmup.GetStringAsync(address);
        string script = html.Split("<script src=\"")[1].Split('"')[0];
        string framework = await warmup.GetStringAsync(new Uri(new Uri(address), script));
        Check.That(framework.Length > 1000, "Blazor framework asset is served before browser navigation");
    }
    var browser = await Harness.Run("node", "browser-driver.cjs", address, buffered ? "buffered" : "streaming");
    Console.WriteLine(browser.Text);
    Check.That(browser.Code == 0, "real browser receives multiple model updates", "AI streaming response was buffered");
    AiRuntime.Evidence("ai-evidence.json", new { passed = true, actualBrowser = true, buffered = false, address });
}
finally { await app.StopAsync(); }
