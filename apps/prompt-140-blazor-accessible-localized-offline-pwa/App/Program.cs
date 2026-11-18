using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
var mode = args.FirstOrDefault() ?? "serve";
var directory = Path.GetFullPath("artifacts/pwa/wwwroot");
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
var app = builder.Build();
var types = new FileExtensionContentTypeProvider();
types.Mappings[".dat"] = "application/octet-stream";
types.Mappings[".wasm"] = "application/wasm";
var provider = new PhysicalFileProvider(directory);
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = provider });
app.UseStaticFiles(new StaticFileOptions { FileProvider = provider, ContentTypeProvider = types });
app.MapGet("/api/status", (HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { available = true });
});
app.MapFallback(async context => await context.Response.SendFileAsync(Path.Combine(directory,"index.html")));
await app.StartAsync();
try
{
    if (mode == "serve")
    {
        Console.WriteLine($"Published standalone PWA {app.Urls.Single()}");
        await app.WaitForShutdownAsync();
    }
    else
    {
        await BrowserRunner.RunAsync(app.Urls.Single(), "140", mode);
        Console.WriteLine("PASS published WebAssembly PWA verified");
    }
}
finally { await app.StopAsync(); await app.DisposeAsync(); }
