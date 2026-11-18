using Microsoft.AspNetCore.Identity;
public static class AuthenticatorEnrollment
{
    public static async Task<bool> Enable(UserManager<CourseUser> users,CourseUser user,string currentPassword,string code)
    {
        if(!await users.IsEmailConfirmedAsync(user) || !await users.CheckPasswordAsync(user,currentPassword))return false;
        if(!await users.VerifyTwoFactorTokenAsync(user,TokenOptions.DefaultAuthenticatorProvider,code))return false;
        return (await users.SetTwoFactorEnabledAsync(user,true)).Succeeded;
    }
}
