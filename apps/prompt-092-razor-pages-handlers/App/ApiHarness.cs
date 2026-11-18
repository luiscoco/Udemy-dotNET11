using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
public sealed class ApiHarness(WebApplication app) : IAsyncDisposable
{
    public WebApplication App { get; } = app;
    public HttpClient Client { get; } = new() { BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(30) };
    public static async Task<ApiHarness> StartAsync(Action<WebApplicationBuilder> configure, Action<WebApplication> map)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        { EnvironmentName = "Production", ApplicationName = typeof(ApiHarness).Assembly.GetName().Name, ContentRootPath = Path.GetFullPath("App") });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        configure(builder); var app = builder.Build();
        app.MapGet("/__warmup", () => "ready"); map(app);
        await app.StartAsync(); var harness = new ApiHarness(app);
        using var warmup = await harness.Client.GetAsync("/__warmup"); warmup.EnsureSuccessStatusCode();
        return harness;
    }
    public async ValueTask DisposeAsync() { Client.Dispose(); await App.StopAsync(); await App.DisposeAsync(); }
}
