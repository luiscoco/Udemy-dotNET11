using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// Local test identities only. This header is not production authentication.
public sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string user=Request.Headers["X-Test-User"].ToString();
        if(user is not ("alice" or "bob")) return Task.FromResult(AuthenticateResult.NoResult());
        var identity=new ClaimsIdentity([new Claim(ClaimTypes.Name,user)],Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity),Scheme.Name)));
    }
}
public sealed class EndpointCounters { public int Public,Personal; }
public sealed record PublicReply(int Generation);
public sealed record PersonalReply(string User,int Generation);
