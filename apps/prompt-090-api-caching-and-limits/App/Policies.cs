using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
public static class Policies
{
    public static void Configure(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<EndpointCounters>();
        builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions,FixtureAuthentication>("fixture",_=>{});
        builder.Services.AddAuthorization(); builder.Services.AddOutputCache();
        builder.Services.AddResponseCompression(o=> { o.Providers.Add<GzipCompressionProvider>(); o.MimeTypes=["text/plain"]; });
        builder.Services.AddRateLimiter(o=>
        {
            o.RejectionStatusCode=429;
            o.AddPolicy("limited",context=>RateLimitPartition.GetFixedWindowLimiter(context.User.Identity?.Name??"anonymous",
                _=>new FixedWindowRateLimiterOptions {PermitLimit=2,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true}));
        });
    }
    public static void Map(WebApplication app)
    {
        app.UseResponseCompression(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
        app.UseRateLimiter(); app.UseOutputCache();
        app.MapGet("/public",(EndpointCounters c)=>new PublicReply(Interlocked.Increment(ref c.Public))).CacheOutput();
        app.MapGet("/me",(ClaimsPrincipal user,EndpointCounters c)=>new PersonalReply(user.Identity!.Name!,Interlocked.Increment(ref c.Personal)))
            .RequireAuthorization().CacheOutput(policy=>policy.NoCache());
        app.MapGet("/text",()=>Results.Text(new string('x',4096),"text/plain"))
            .CacheOutput(policy=>policy.SetVaryByHeader("Accept-Encoding"));
        app.MapGet("/limited",()=>Results.Ok("admitted")).RequireAuthorization().RequireRateLimiting("limited");
    }
}
