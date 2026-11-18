using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using var orders=new OrderStore();const string injection="does-not-exist' OR 1=1 --";
Check.That(orders.Search("alice",injection)==0,"parameterized SQL treats the injected condition only as data");
Check.That(orders.Search("alice",injection,concatenate:true)==2,"controlled unsafe concatenation exposes both fixture rows");
int privateHits=0;await using var catalog=new SecureHarness();
await catalog.StartAsync(_=>{},app=>
{
    app.MapGet("/catalog",(HttpContext context)=>context.Request.Query.ContainsKey("redirect")?Results.Redirect("/admin"):Results.Text("Public course catalog"));
    app.MapGet("/admin",()=>{Interlocked.Increment(ref privateHits);return Results.Text("Synthetic internal-only data");});
});
using var outbound=catalog.CreateClient(redirects:false);var reader=new CatalogReader(new Uri(catalog.Address),outbound);
Check.That(await reader.Fetch(catalog.Address+"/catalog")=="Public course catalog","allowlisted owned catalog request succeeds");
foreach(var target in new[]{"http://169.254.169.254/latest/meta-data",catalog.Address+"/admin",catalog.Address+"/catalog?redirect=1",catalog.Address.Replace("127.0.0.1","127.0.0.1.attacker.test")+"/catalog"})
{
    bool blocked=false;try{await reader.Fetch(target);}catch(UnauthorizedAccessException){blocked=true;}
    Check.That(blocked,"unapproved URL or redirect is rejected before reaching the internal target");
}
Check.That(privateHits==0,"SSRF redirect test never reaches the owned internal-only endpoint");
using var authority=new TokenAuthority();await using var api=new SecureHarness();
await api.StartAsync(b=>authority.Configure(b.Services),app=>
{
    app.UseAuthentication();app.UseAuthorization();
    app.MapGet("/orders/{id:int}",(int id,HttpContext context)=>orders.Owns(context.User.FindFirst("sub")!.Value,id)?Results.Ok():Results.NotFound()).RequireAuthorization();
});
using var client=api.CreateClient();
Check.That((await client.GetAsync("/orders/1")).StatusCode==HttpStatusCode.Unauthorized,"anonymous request cannot read an order");
client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",authority.Issue(subject:"bob"));
Check.That((await client.GetAsync("/orders/1")).StatusCode==HttpStatusCode.NotFound,"authenticated non-owner cannot read another user's order");
Check.That((await client.GetAsync("/orders/2")).IsSuccessStatusCode,"owner can read the intended order");
using var sink=new SecurityLog(args.FirstOrDefault()=="logging");using var logs=LoggerFactory.Create(b=>b.AddProvider(sink));
var logger=logs.CreateLogger("AccountEvents");const string sentinel="SYNTHETIC_CREDENTIAL_SENTINEL";
logger.LogInformation("Attempt {UserId} {Outcome} {Password}","alice","Rejected",sentinel);
var leaked=sink.Lines.Any(line=>line.Contains(sentinel));
if(args.FirstOrDefault()=="logging")Check.That(!leaked,"incorrect expectation: unrestricted log formatting keeps credentials out");
Check.That(!leaked&&sink.Lines.Any(line=>line.Contains("UserId=alice")),"allowlisted structured logging keeps useful identity context without credential fields");
Console.WriteLine("service: actual local SQL injection, guarded catalog requests, ownership and sensitive-log regression checks passed");
