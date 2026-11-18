using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
public sealed class LocalMail
{public string Confirmation {get;set;}="";public string Reset {get;set;}="";}
public sealed record Credentials(string Email,string Password);
public sealed record AccountCode(string Email,string Code);
public sealed record ResetInput(string Email,string Code,string Password);
public static class AccountEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/register",async(Credentials input,UserManager<CourseUser> users,LocalMail mail)=>
        {
            var user=new CourseUser{UserName=input.Email,Email=input.Email};
            var result=await users.CreateAsync(user,input.Password);
            if(!result.Succeeded)return Results.BadRequest(new{error="Registration rejected"});
            mail.Confirmation=await users.GenerateEmailConfirmationTokenAsync(user);
            return Results.Accepted();
        });
        app.MapPost("/confirm",async(AccountCode input,UserManager<CourseUser> users)=>
        {
            var user=await users.FindByEmailAsync(input.Email);
            return user is not null && (await users.ConfirmEmailAsync(user,input.Code)).Succeeded?Results.Ok():Results.BadRequest();
        });
        app.MapPost("/login",async(Credentials input,SignInManager<CourseUser> signin)=>
        {
            var result=await signin.PasswordSignInAsync(input.Email,input.Password,false,lockoutOnFailure:true);
            return result.Succeeded?Results.Ok():Results.StatusCode(result.IsLockedOut?423:401);
        });
        app.MapPost("/forgot",async(AccountCode input,UserManager<CourseUser> users,LocalMail mail)=>
        {
            var user=await users.FindByEmailAsync(input.Email);
            if(user is not null && await users.IsEmailConfirmedAsync(user))mail.Reset=await users.GeneratePasswordResetTokenAsync(user);
            return Results.Accepted(); // Same public response for known and unknown accounts.
        });
        app.MapPost("/reset",async(ResetInput input,UserManager<CourseUser> users)=>
        {
            var user=await users.FindByEmailAsync(input.Email);
            return user is not null && (await users.ResetPasswordAsync(user,input.Code,input.Password)).Succeeded?Results.Ok():Results.BadRequest();
        });
        app.MapGet("/me",(System.Security.Claims.ClaimsPrincipal user)=>Results.Text(user.Identity!.Name!)).RequireAuthorization();
    }
}
