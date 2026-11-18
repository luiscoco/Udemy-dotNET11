using System.Net;
using System.Net.Http.Json;
public static class AccountChecks
{
    public static async Task Run(HttpClient client,LocalMail mail,string mode)
    {
        const string email="student@example.test",password="Original_11!Password",changed="Changed_11!Password";
        Check.That((await client.PostAsJsonAsync("/register",new Credentials(email,password))).StatusCode==HttpStatusCode.Accepted,"registration persists an Identity account and stages confirmation");
        Check.That((await client.PostAsJsonAsync("/login",new Credentials(email,password))).StatusCode==HttpStatusCode.Unauthorized,"unconfirmed account cannot sign in");
        (await client.PostAsJsonAsync("/confirm",new AccountCode(email,mail.Confirmation))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/login",new Credentials(email,password))).EnsureSuccessStatusCode();
        Check.That(await client.GetStringAsync("/me")==email,"confirmed login creates an authenticated session");
        var known=await client.PostAsJsonAsync("/forgot",new AccountCode(email,""));
        var unknown=await client.PostAsJsonAsync("/forgot",new AccountCode("missing@example.test",""));
        Check.That(known.StatusCode==unknown.StatusCode && known.StatusCode==HttpStatusCode.Accepted,"forgot-password response does not reveal account existence");
        var code=mail.Reset;
        (await client.PostAsJsonAsync("/reset",new ResetInput(email,code,changed))).EnsureSuccessStatusCode();
        Check.That((await client.PostAsJsonAsync("/reset",new ResetInput(email,code,"Another_11!Password"))).StatusCode==HttpStatusCode.BadRequest,"consumed reset token cannot be reused after the security stamp changes");
        Check.That((await client.PostAsJsonAsync("/login",new Credentials(email,password))).StatusCode==HttpStatusCode.Unauthorized,"old password stops authenticating after reset");
        (await client.PostAsJsonAsync("/login",new Credentials(email,changed))).EnsureSuccessStatusCode();
        const string locked="locked@example.test";
        (await client.PostAsJsonAsync("/register",new Credentials(locked,password))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/confirm",new AccountCode(locked,mail.Confirmation))).EnsureSuccessStatusCode();
        for(int i=0;i<3;i++)await client.PostAsJsonAsync("/login",new Credentials(locked,"Wrong_11!Password"));
        var response=await client.PostAsJsonAsync("/login",new Credentials(locked,password));
        if(mode=="lockout")Check.That(response.IsSuccessStatusCode,"incorrect expectation: a locked account accepts the correct password");
        Check.That((int)response.StatusCode==423,"correct password does not bypass the active account lockout");
        Console.WriteLine("service: Identity registration, confirmation, login, reset replay rejection and lockout verified");
    }
}
