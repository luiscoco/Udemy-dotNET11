using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public sealed class PipelineProbe {public int AfterRouting;}
public static class AssetRoutes
{
    public static void Map(WebApplication app,PipelineProbe probe,string html)
    {
        app.Use(async(context,next)=>
        {
            if(context.Request.Path.StartsWithSegments("/css")||context.Request.Path.StartsWithSegments("/js"))
                context.Response.OnStarting(()=>{context.Response.Headers.CacheControl="public,max-age=60";return Task.CompletedTask;});
            await next(context);
        });
        app.UseRouting();app.Use(async(context,next)=>{Interlocked.Increment(ref probe.AfterRouting);await next(context);});
        app.MapStaticAssets().ShortCircuit();
        app.MapGet("/",()=>Results.Content(html,"text/html"));app.MapGet("/second",()=>Results.Content(html,"text/html"));
    }
}
