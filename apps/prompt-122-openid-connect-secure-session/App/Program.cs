using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
var folder=Path.GetFullPath(Path.Combine("artifacts","oidc-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
await using var server=new SecureHarness();
await server.StartAsync(b=>
{
    SessionSetup.Add(b);b.Services.AddDataProtection().SetApplicationName("course122").PersistKeysToFileSystem(new DirectoryInfo(folder)).ProtectKeysWithCertificate(server.Certificate);
},app=>
{
    app.UseAuthentication();app.UseAuthorization();
    app.MapGet("/",()=>Results.Content(Page.Home,"text/html"));
    app.MapGet("/login",()=>Results.Challenge(new AuthenticationProperties{RedirectUri="/profile"},["oidc"]));
    app.MapGet("/profile",(HttpContext context,IAntiforgery antiforgery)=>
    {
        var token=antiforgery.GetAndStoreTokens(context).RequestToken!;
        return Results.Content(Page.Profile(HtmlEncoder.Default.Encode(context.User.Identity!.Name!),HtmlEncoder.Default.Encode(token)),"text/html");
    }).RequireAuthorization();
    app.MapGet("/me",(HttpContext context)=>context.User.Identity?.IsAuthenticated==true?Results.Ok():Results.Unauthorized());
    app.MapPost("/logout",async(HttpContext context,IAntiforgery antiforgery)=>
    {
        await antiforgery.ValidateRequestAsync(context);await context.SignOutAsync("session");return Results.NoContent();
    }).RequireAuthorization();
},environment:"LocalProviderLab",port:18122);
using var client=server.CreateClient();
using var challenge=await client.GetAsync("/login");
Check.That(challenge.StatusCode==HttpStatusCode.Redirect,"OIDC challenge redirects to the actual local Keycloak provider");
var location=challenge.Headers.Location!.ToString();
Check.That(location.Contains("code_challenge_method=S256") && location.Contains("state=") && location.Contains("nonce="),"challenge includes S256 PKCE, protected state and nonce");
using var rejected=await client.GetAsync("/signin-oidc?code=fake&state=invalid");
if(args.FirstOrDefault()=="callback")Check.That(rejected.IsSuccessStatusCode,"incorrect expectation: an unsolicited callback authenticates");
Check.That(rejected.StatusCode==HttpStatusCode.BadRequest,"unsolicited OIDC callback is rejected before creating a session");
if(args.FirstOrDefault()=="browser")await BrowserRunner.RunAsync(server.Address,"oidc",server.SpkiPin);
Console.WriteLine("service: actual provider challenge and callback defenses verified; HTTP metadata is a local-only fixture setting");
