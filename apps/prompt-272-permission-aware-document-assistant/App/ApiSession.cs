using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

public sealed class ApiSession : IAsyncDisposable
{
    public WebApplication App { get; }
    public HttpClient Client { get; }

    public ApiSession(WebApplication app)
    {
        App = app;
        Client = new()
        {
            BaseAddress = new Uri(app.Urls.Single()),
            Timeout = TimeSpan.FromSeconds(90)
        };
    }

    public void Token(string value) => Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
    public static WebApplicationBuilder Builder()
    {
        var b = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(ApiSession).Assembly.FullName, EnvironmentName = "Production" });
        b.Logging.ClearProviders();
        b.WebHost.UseUrls("http://127.0.0.1:0");
        return b;
    }

    public static async Task<ApiSession> Start(WebApplication app)
    {
        await app.StartAsync();
        return new(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}
