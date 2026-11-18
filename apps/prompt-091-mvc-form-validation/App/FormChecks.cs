using System.Net;
using System.Text.RegularExpressions;
public static class FormChecks
{
    public static string Token(string html) => WebUtility.HtmlDecode(Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    public static async Task VerifyAsync(Uri address,bool pages,string? mode)
    {
        using var client=new HttpClient(new HttpClientHandler {AllowAutoRedirect=false}) {BaseAddress=address};
        string route=pages?"/Join":"/enroll/create", post=pages?"/Join?handler=Reserve":route, prefix=pages?"Input.":"";
        string html=await client.GetStringAsync(route); string token=Token(html);
        Check.That(token.Length>20,"the rendered form includes an antiforgery token");
        using var invalid=await client.PostAsync(post,new FormUrlEncodedContent(new Dictionary<string,string>
            { [prefix+"Name"]="Ada",[prefix+"Seats"]="0",["__RequestVerificationToken"]=token }));
        string errors=await invalid.Content.ReadAsStringAsync();
        Console.WriteLine($"browser: invalid form HTTP {(int)invalid.StatusCode}; field error present {errors.Contains("Seats must be between 1 and 4.")}");
        if(mode=="invalid") Check.That((int)invalid.StatusCode==302,"invalid model state must redisplay the form instead of redirecting");
        Check.That((int)invalid.StatusCode==200 && errors.Contains("Seats must be between 1 and 4."),"server validation redisplays the bound model with its field error");
        using var valid=await client.PostAsync(post,new FormUrlEncodedContent(new Dictionary<string,string>
            { [prefix+"Name"]="<script>test</script>",[prefix+"Seats"]="2",["__RequestVerificationToken"]=Token(errors) }));
        Check.That((int)valid.StatusCode==302 && valid.Headers.Location is not null,"valid input uses post-redirect-get");
        string success=await client.GetStringAsync(valid.Headers.Location);
        Check.That(success.Contains("Enrollment confirmed") && success.Contains("&lt;script&gt;") && !success.Contains("<script>test</script>"),"the confirmation view HTML-encodes submitted text");
        using var missingToken=await client.PostAsync(post,new FormUrlEncodedContent(new Dictionary<string,string> {[prefix+"Name"]="Ada",[prefix+"Seats"]="2"}));
        Check.That((int)missingToken.StatusCode==400,"a state-changing form without its antiforgery token is rejected");
    }
}
