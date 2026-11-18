using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
public static class CourseWeb
{
    public static WebApplication Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.AddAntiforgery();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<CourseState>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Events.OnRedirectToLogin = context => { context.Response.Redirect(context.RedirectUri); return Task.CompletedTask; };
        });
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapGet("/login", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Content(Login(context,antiforgery), "text/html"));
        app.MapPost("/login", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var form = await context.Request.ReadFormAsync();
            if (form["student"] != "student" || form["password"] != "course-demo")
                return Results.Content(Login(context,antiforgery,"Credentials were rejected."), "text/html", statusCode:401);
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name,"student")], CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Results.Redirect("/dashboard");
        });
        app.MapGet("/dashboard", (HttpContext context, IAntiforgery antiforgery, CourseState state) =>
            Results.Content(Dashboard(context,antiforgery,state),"text/html")).RequireAuthorization();
        app.MapPost("/courses", async (HttpContext context, IAntiforgery antiforgery, CourseState state) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            string title = (await context.Request.ReadFormAsync())["title"].ToString();
            if (string.IsNullOrWhiteSpace(title))
                return Results.Content(Dashboard(context,antiforgery,state,"A course title is required."),"text/html",statusCode:400);
            state.Title = title.Trim();
            return Results.Redirect("/dashboard");
        }).RequireAuthorization();
        return app;
    }
    private static string Token(HttpContext context, IAntiforgery antiforgery)
        => $"<input type='hidden' name='__RequestVerificationToken' value='{WebUtility.HtmlEncode(antiforgery.GetAndStoreTokens(context).RequestToken)}'>";
    private static string Login(HttpContext context, IAntiforgery antiforgery, string error = "")
        => Page("Sign in", $$"""
            <p>Use the local demonstration account to continue.</p>
            {{Alert(error)}}
            <form method="post" action="/login">
                {{Token(context,antiforgery)}}
                <label for="student">Student name</label>
                <input id="student" name="student" autocomplete="username">
                <label for="password">Password</label>
                <input id="password" name="password" type="password" autocomplete="current-password">
                <button>Sign in</button>
            </form>
            <p class="hint">Demo account · student / course-demo</p>
            """);
    private static string Dashboard(HttpContext context, IAntiforgery antiforgery, CourseState state, string error = "")
        => Page("Your course", $$"""
            <p>Signed in as student.</p>
            {{Alert(error)}}
            <form method="post" action="/courses">
                {{Token(context,antiforgery)}}
                <label for="title">Course title</label>
                <input id="title" name="title">
                <button>Save course</button>
            </form>
            <p role="status" data-testid="saved">{{(state.Title is null ? "Choose a title to save." : "Saved " + WebUtility.HtmlEncode(state.Title))}}</p>
            """);
    private static string Alert(string text)
        => text.Length == 0 ? "" : $"<p role='alert'>{WebUtility.HtmlEncode(text)}</p>";
    private static string Page(string title, string body) => $$"""
        <!doctype html>
        <html lang="en">
        <meta charset="utf-8">
        <title>{{title}}</title>
        <style>
            body {
                margin: 0;
                background: #f3effa;
                color: #20143e;
                font: 20px/1.5 Inter, system-ui, sans-serif;
            }
            main {
                max-width: 660px;
                margin: 65px auto;
                padding: 44px;
                background: white;
                border: 1px solid #d8cbe8;
                border-radius: 20px;
            }
            h1 { font-size: 38px; line-height: 1.15; }
            label { display: block; margin-top: 20px; font-weight: 600; }
            input {
                box-sizing: border-box;
                width: 100%;
                font: inherit;
                padding: 12px;
                border: 1px solid #a993c5;
                border-radius: 8px;
            }
            button {
                margin-top: 25px;
                padding: 13px 24px;
                background: #6637aa;
                color: white;
                border: 0;
                border-radius: 8px;
                font: inherit;
            }
            [role=alert] {
                padding: 15px;
                color: #9b1233;
                background: #fff0f3;
                border-left: 4px solid #c22;
            }
            [role=status] { padding: 15px; background: #e6f7ef; }
            .hint { font-size: 16px; color: #63556f; }
        </style>
        <main>
            <p>.NET 11 · Browser testing</p>
            <h1>{{title}}</h1>
            {{body}}
        </main>
        </html>
        """;

}
public sealed class CourseState { public string? Title { get; set; } }
