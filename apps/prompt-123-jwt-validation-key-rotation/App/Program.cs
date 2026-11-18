using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using var authority=new TokenAuthority();
await using var server=await ApiHarness.StartAsync(b=>authority.Configure(b.Services),app=>
{app.UseAuthentication();app.UseAuthorization();app.MapGet("/private",()=>Results.Text("Authorized course data")).RequireAuthorization();});
async Task<HttpStatusCode> Status(string token)
{
    using var request=new HttpRequestMessage(HttpMethod.Get,"/private");request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
    using var response=await server.Client.SendAsync(request);return response.StatusCode;
}
var old=authority.Issue();
Check.That(await Status(old)==HttpStatusCode.OK,"valid issuer, audience, lifetime and trusted signature authenticate");
var wrongIssuer=await Status(authority.Issue(issuer:"https://other.course.test"));
if(args.FirstOrDefault()=="issuer")Check.That(wrongIssuer==HttpStatusCode.OK,"incorrect expectation: a different issuer is trusted");
Check.That(wrongIssuer==HttpStatusCode.Unauthorized,"wrong issuer is rejected with HTTP 401");
Check.That(await Status(authority.Issue(audience:"another-api"))==HttpStatusCode.Unauthorized,"wrong audience is rejected");
Check.That(await Status(authority.Issue(expired:true))==HttpStatusCode.Unauthorized,"expired token is rejected without an implicit clock-skew allowance");
Check.That(await Status(authority.Issue(key:"B"))==HttpStatusCode.Unauthorized,"unpublished signing key is rejected");
authority.Trust("A","B");var fresh=authority.Issue(key:"B");
Check.That(await Status(fresh)==HttpStatusCode.OK && await Status(old)==HttpStatusCode.OK,"rollover overlap accepts both explicitly trusted keys");
authority.Trust("B");
Check.That(await Status(old)==HttpStatusCode.Unauthorized && await Status(fresh)==HttpStatusCode.OK,"retiring key A rejects old signatures while key B remains valid");
Check.That((await server.Client.GetAsync("/private")).StatusCode==HttpStatusCode.Unauthorized,"missing bearer credentials do not reach protected data");
Console.WriteLine("service: JWT validation and controlled key rollover verified; token contents were not logged");
