using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
public static class ProtectionSetup
{
    public const string Frontend="https://student.course.test";
    public static void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddAntiforgery(o=>
        {
            o.HeaderName="X-CSRF-TOKEN";o.Cookie.Name="__Host-CourseAnti";
            o.Cookie.SecurePolicy=CookieSecurePolicy.Always;o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Strict;
        });
        builder.Services.AddCors(o=>o.AddPolicy("frontend",p=>p.WithOrigins(Frontend)
            .WithMethods("POST").WithHeaders("Content-Type","X-CSRF-TOKEN").AllowCredentials()));
    }
}
