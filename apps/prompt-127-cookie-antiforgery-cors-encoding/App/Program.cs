using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
var folder=Path.GetFullPath(Path.Combine("artifacts","protection-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
await using var server=new SecureHarness();
await server.StartAsync(b=>
{
    IdentitySetup.Add(b.Services,Path.Combine(folder,"accounts.db"));ProtectionSetup.Add(b);
    b.Services.AddDataProtection().SetApplicationName("course127").PersistKeysToFileSystem(new DirectoryInfo(folder)).ProtectKeysWithCertificate(server.Certificate);
},app=>
{
    app.UseCors("frontend");app.UseAuthentication();app.UseAuthorization();
    app.MapPost("/login",async(SignInManager<CourseUser> manager)=>
    {var result=await manager.PasswordSignInAsync("student@example.test","Original_11!Password",false,true);return Results.StatusCode(result.Succeeded?200:401);});
    app.MapGet("/token",(HttpContext context,IAntiforgery antiforgery)=>Results.Json(new{token=antiforgery.GetAndStoreTokens(context).RequestToken})).RequireAuthorization();
    app.MapPost("/update",async(HttpContext context,IAntiforgery antiforgery)=>
    {
        try{await antiforgery.ValidateRequestAsync(context);}catch(AntiforgeryValidationException){return Results.BadRequest();}
        return Results.Ok(new{saved=true});
    }).RequireAuthorization();
    app.MapGet("/",(HttpContext context)=>
    {
        var nonce=Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        context.Response.Headers.ContentSecurityPolicy=$"default-src 'self'; script-src 'nonce-{nonce}'; style-src 'unsafe-inline'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";
        return Results.Content(Page.Html(nonce),"text/html");
    });
});
await IdentitySetup.Initialize(server.App.Services);
using(var scope=server.App.Services.CreateScope())
{var users=scope.ServiceProvider.GetRequiredService<UserManager<CourseUser>>();Check.That((await users.CreateAsync(new(){UserName="student@example.test",Email="student@example.test",EmailConfirmed=true},"Original_11!Password")).Succeeded,"fixture creates the authenticated browser account");}
using var client=server.CreateClient();
Check.That((await client.PostAsync("/update",null)).StatusCode==HttpStatusCode.Unauthorized,"unauthenticated mutation is rejected before antiforgery validation");
using var login=await client.PostAsync("/login",null);login.EnsureSuccessStatusCode();
var cookie=login.Headers.GetValues("Set-Cookie").Single(x=>x.StartsWith("__Host-CourseSession="));
Check.That(cookie.Contains("secure",StringComparison.OrdinalIgnoreCase)&&cookie.Contains("httponly",StringComparison.OrdinalIgnoreCase)&&cookie.Contains("samesite=lax",StringComparison.OrdinalIgnoreCase),"session cookie is Secure, HttpOnly and SameSite=Lax");
var rejected=await client.PostAsync("/update",null);
if(args.FirstOrDefault()=="csrf")Check.That(rejected.IsSuccessStatusCode,"incorrect expectation: the cookie alone authorizes a state-changing request");
Check.That(rejected.StatusCode==HttpStatusCode.BadRequest,"authenticated mutation without the request token is rejected");
var response=await client.GetFromJsonAsync<TokenReply>("/token");
using var valid=new HttpRequestMessage(HttpMethod.Post,"/update");valid.Headers.Add("X-CSRF-TOKEN",response!.Token);
Check.That((await client.SendAsync(valid)).IsSuccessStatusCode,"cookie plus matching antiforgery request token permits the mutation");
async Task<bool> Preflight(string origin)
{
    using var request=new HttpRequestMessage(HttpMethod.Options,"/update");request.Headers.Add("Origin",origin);
    request.Headers.Add("Access-Control-Request-Method","POST");request.Headers.Add("Access-Control-Request-Headers","X-CSRF-TOKEN");
    using var result=await client.SendAsync(request);return result.Headers.TryGetValues("Access-Control-Allow-Origin",out var values)&&values.Single()==origin;
}
Check.That(await Preflight(ProtectionSetup.Frontend)&&!await Preflight("https://attacker.course.test"),"CORS preflight permits only the explicit frontend origin");
var html=await client.GetStringAsync("/");
Check.That(html.Contains("&lt;img")&&!html.Contains("<img src=x"),"server encodes untrusted display text rather than inserting executable markup");
if(args.FirstOrDefault()=="browser")await BrowserRunner.RunAsync(server.Address,"antiforgery",server.SpkiPin);
Console.WriteLine("service: cookie flags, antiforgery, CORS headers and encoded output verified as distinct protections");
public sealed record TokenReply(string Token);
