using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
                options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
            });
        builder.Services.AddAuthorization(options => options.AddPolicy("edit", policy => policy.RequireClaim("permission", "course.edit")));
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        builder.Services.AddScoped<CourseActions>();
    }
    public static void Map(WebApplication app)
    {
        // Synthetic identities exist only in this loopback Development fixture.
        app.MapPost("/fixture/signin/{role}", async (HttpContext context, string role) =>
        {
            if (role is not ("reader" or "editor")) return Results.BadRequest();
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, role), new(ClaimTypes.Name, role) };
            if (role == "editor") claims.Add(new("permission", "course.edit"));
            await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
            return Results.Ok();
        });
        app.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken })).RequireAuthorization();
        app.MapPost("/api/publish", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.BadRequest(); }
            return Results.Ok(new { published = true });
        }).RequireAuthorization("edit");
    }
}
