using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
public static class Gateway
{
    public static void Configure(WebApplicationBuilder builder, string destination)
    {
        builder.Services.AddAuthentication("local-demo").AddScheme<AuthenticationSchemeOptions,DemoAuthentication>("local-demo", _ => {});
        builder.Services.AddAuthorization(options => options.AddPolicy("course-reader", policy => policy.RequireAuthenticatedUser().RequireClaim("course-access","read")));
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 503;
            options.AddConcurrencyLimiter("bounded", limiter => { limiter.PermitLimit=1; limiter.QueueLimit=0; });
        });
        builder.Services.AddReverseProxy().LoadFromMemory(
            [new RouteConfig { RouteId="courses", ClusterId="catalog", AuthorizationPolicy="course-reader",
                Match=new RouteMatch {Path="/gateway/{**rest}"},
                Transforms=[new Dictionary<string,string>{{"PathRemovePrefix","/gateway"}}] }],
            [new ClusterConfig { ClusterId="catalog", HttpRequest=new ForwarderRequestConfig {ActivityTimeout=TimeSpan.FromSeconds(2)},
                Destinations=new Dictionary<string,DestinationConfig>{{"primary",new(){Address=destination}}} }]);
    }
    public static void Map(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapReverseProxy().RequireRateLimiting("bounded");
    }
}
