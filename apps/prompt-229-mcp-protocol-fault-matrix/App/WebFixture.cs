using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
public sealed class WebFixture(WebApplication app) : IAsyncDisposable
{
    public WebApplication App { get; } = app;
    public Uri BaseUri => new(App.Urls.Single());
    public Uri McpUri => new(BaseUri,"/mcp");
    public static WebApplicationBuilder Builder()
    {
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions
        { ApplicationName=typeof(WebFixture).Assembly.FullName,EnvironmentName="Production" });
        builder.Configuration["AllowedHosts"]="127.0.0.1;localhost";
        builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:0");return builder;
    }
    public static async Task<WebFixture> Start(WebApplication app)
    { await app.StartAsync();return new(app); }
    public HttpClient Client(string? bearer=null)
    {
        var http=new HttpClient { BaseAddress=BaseUri,Timeout=TimeSpan.FromSeconds(45) };
        if(bearer!=null)http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",bearer);
        return http;
    }
    public async ValueTask DisposeAsync()
    {
        using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await App.StopAsync(stop.Token);await App.DisposeAsync();
    }
}
