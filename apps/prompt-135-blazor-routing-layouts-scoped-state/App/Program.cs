using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Course.Components;
var mode = args.FirstOrDefault() ?? "serve";
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Path.GetFullPath("App"),
    ApplicationName = typeof(Program).Assembly.GetName().Name,
    EnvironmentName = "Development"
});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
LessonSetup.Add(builder);
var app = builder.Build();
app.UseStaticFiles();

app.UseAntiforgery();
app.MapStaticAssets();
LessonSetup.Map(app);
app.MapRazorComponents<AppShell>().AddInteractiveServerRenderMode();
await app.StartAsync();
try
{
    var address = app.Urls.Single();
    if (mode == "serve")
    {
        Console.WriteLine($"Blazor application {address}");
        await app.WaitForShutdownAsync();
    }
    else
    {
        await BrowserRunner.RunAsync(address, "135", mode);
        Console.WriteLine("PASS actual Blazor browser assertions completed");
    }
}
finally
{
    await app.StopAsync();
    await app.DisposeAsync();
}
