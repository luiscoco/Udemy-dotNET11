using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
public static class WorkHost
{
    public static WebApplication Create(WorkGate gate)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();
        app.MapGet("/work", (CancellationToken token) => gate.ExecuteAsync(token));
        return app;
    }
}
