using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
public static class SessionSetup
{
    public static void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(o=>{o.DefaultScheme="session";o.DefaultChallengeScheme="oidc";})
            .AddCookie("session",o=>
            {
                o.Cookie.Name="__Host-OidcSession";o.Cookie.SecurePolicy=CookieSecurePolicy.Always;
                o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;
                o.ExpireTimeSpan=TimeSpan.FromMinutes(20);o.SlidingExpiration=false;
                o.Events.OnRedirectToLogin=c=>{c.Response.StatusCode=401;return Task.CompletedTask;};
            }).AddOpenIdConnect("oidc",o=>
            {
                o.Authority="http://127.0.0.1:18088/realms/course";o.ClientId="course-web";
                o.ResponseType="code";o.ResponseMode="query";o.UsePkce=true;
                // Keep parameters visible in this teaching trace; production may retain optional PAR.
                o.PushedAuthorizationBehavior=PushedAuthorizationBehavior.Disable;
                o.SaveTokens=false;o.MapInboundClaims=false;o.Scope.Clear();o.Scope.Add("openid");o.Scope.Add("profile");
                o.TokenValidationParameters.ValidateIssuer=true;o.TokenValidationParameters.NameClaimType="preferred_username";
                o.ProtocolValidator.RequireNonce=true;o.RequireHttpsMetadata=true;
                if(builder.Environment.EnvironmentName=="LocalProviderLab")o.RequireHttpsMetadata=false;
                o.Events.OnRemoteFailure=async c=>{c.HandleResponse();c.Response.StatusCode=400;await c.Response.WriteAsync("OIDC callback rejected");};
            });
        builder.Services.AddAuthorization();builder.Services.AddAntiforgery(o=>o.HeaderName="X-CSRF-TOKEN");
    }
}
