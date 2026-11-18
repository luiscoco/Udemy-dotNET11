using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
var folder=Path.GetFullPath(Path.Combine("artifacts","mfa-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
await using var server=new SecureHarness();
await server.StartAsync(b=>
{
    IdentitySetup.Add(b.Services,Path.Combine(folder,"accounts.db"));
    b.Services.Configure<SecurityStampValidatorOptions>(o=>
    {
        o.ValidationInterval=TimeSpan.Zero; // Immediate revocation is an explicit fixture policy.
        o.OnRefreshingPrincipal=context=>
        {
            if(context.NewPrincipal?.Identity is System.Security.Claims.ClaimsIdentity refreshed && context.CurrentPrincipal is {} current)
                refreshed.AddClaims(current.FindAll("amr"));
            return Task.CompletedTask;
        };
    });
    b.Services.AddAuthorization(o=>o.AddPolicy("Mfa",p=>p.RequireAuthenticatedUser().RequireClaim("amr","mfa")));
    b.Services.AddDataProtection().SetApplicationName("course126").PersistKeysToFileSystem(new DirectoryInfo(folder)).ProtectKeysWithCertificate(server.Certificate);
},app=>
{
    app.UseAuthentication();app.UseAuthorization();
    app.MapPost("/login",async(Login input,SignInManager<CourseUser> manager)=>
    {var r=await manager.PasswordSignInAsync(input.Email,input.Password,false,true);return Results.StatusCode(r.RequiresTwoFactor?202:r.Succeeded?200:401);});
    app.MapPost("/totp",async(Code input,SignInManager<CourseUser> manager)=>
    {var r=await manager.TwoFactorAuthenticatorSignInAsync(input.Value,false,false);return Results.StatusCode(r.Succeeded?200:401);});
    app.MapPost("/recovery",async(Code input,SignInManager<CourseUser> manager)=>
    {var r=await manager.TwoFactorRecoveryCodeSignInAsync(input.Value);return Results.StatusCode(r.Succeeded?200:401);});
    app.MapPost("/logout",async(SignInManager<CourseUser> manager)=>{await manager.SignOutAsync();return Results.NoContent();});
    app.MapGet("/me",()=>Results.Text("MFA-protected course account")).RequireAuthorization("Mfa");
});
await IdentitySetup.Initialize(server.App.Services);
const string email="mfa@example.test",password="Original_11!Password",changed="Changed_11!Password";
string secret;string[] recovery;
using(var scope=server.App.Services.CreateScope())
{
    var users=scope.ServiceProvider.GetRequiredService<UserManager<CourseUser>>();
    var user=new CourseUser{UserName=email,Email=email,EmailConfirmed=true};
    Check.That((await users.CreateAsync(user,password)).Succeeded,"fixture creates a confirmed Identity account");
    await users.ResetAuthenticatorKeyAsync(user);secret=(await users.GetAuthenticatorKeyAsync(user))!;
    Check.That(!await AuthenticatorEnrollment.Enable(users,user,"Wrong_11!Password",AuthenticatorSimulator.Code(secret)),"MFA enrollment requires current-password verification");
    Check.That(await AuthenticatorEnrollment.Enable(users,user,password,AuthenticatorSimulator.Code(secret)),"Identity verifies the authenticator code before enabling MFA");
    recovery=(await users.GenerateNewTwoFactorRecoveryCodesAsync(user,3))!.ToArray();
}
using var client=server.CreateClient();
async Task Password(string value)
{Check.That((await client.PostAsJsonAsync("/login",new Login(email,value))).StatusCode==HttpStatusCode.Accepted,"password stage requires a second factor");}
await Password(password);
Check.That((await client.GetAsync("/me")).StatusCode==HttpStatusCode.Unauthorized,"password alone cannot reach the MFA-protected endpoint");
(await client.PostAsJsonAsync("/totp",new Code(AuthenticatorSimulator.Code(secret)))).EnsureSuccessStatusCode();
Check.That((await client.GetAsync("/me")).IsSuccessStatusCode,"valid TOTP creates an MFA-authenticated session");
await client.PostAsync("/logout",null);await Password(password);
(await client.PostAsJsonAsync("/recovery",new Code(recovery[0]))).EnsureSuccessStatusCode();
await client.PostAsync("/logout",null);await Password(password);
var replay=await client.PostAsJsonAsync("/recovery",new Code(recovery[0]));
if(args.FirstOrDefault()=="recovery")Check.That(replay.IsSuccessStatusCode,"incorrect expectation: a consumed recovery code can be reused");
Check.That(replay.StatusCode==HttpStatusCode.Unauthorized,"recovery code is single-use");
using(var scope=server.App.Services.CreateScope())
{
    var users=scope.ServiceProvider.GetRequiredService<UserManager<CourseUser>>();var user=(await users.FindByEmailAsync(email))!;
    await users.GenerateNewTwoFactorRecoveryCodesAsync(user,2);
    Check.That(!(await users.RedeemTwoFactorRecoveryCodeAsync(user,recovery[1])).Succeeded,"regenerating recovery codes invalidates the previous unused set");
    var reset=await users.GeneratePasswordResetTokenAsync(user);
    Check.That((await users.ResetPasswordAsync(user,reset,changed)).Succeeded && await users.GetTwoFactorEnabledAsync(user),"password reset does not disable the enrolled second factor");
}
await client.PostAsync("/logout",null);await Password(changed);
(await client.PostAsJsonAsync("/totp",new Code(AuthenticatorSimulator.Code(secret)))).EnsureSuccessStatusCode();
using(var scope=server.App.Services.CreateScope())
{var users=scope.ServiceProvider.GetRequiredService<UserManager<CourseUser>>();await users.UpdateSecurityStampAsync((await users.FindByEmailAsync(email))!);}
Check.That((await client.GetAsync("/me")).StatusCode==HttpStatusCode.Unauthorized,"security-stamp change revokes the existing session under the fixture's immediate validation policy");
Console.WriteLine("service: TOTP, single-use recovery, recovery rotation and session revocation verified without logging secrets");
public sealed record Login(string Email,string Password);
public sealed record Code(string Value);
