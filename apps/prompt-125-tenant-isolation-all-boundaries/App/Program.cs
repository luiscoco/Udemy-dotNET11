using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var folder=Path.GetFullPath(Path.Combine("artifacts","tenants-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
var options=new DbContextOptionsBuilder<TenantContext>().UseSqlite($"Data Source={Path.Combine(folder,"data.db")};Pooling=False").Options;
await using(var seed=new TenantContext(options,new("seed")))
{await seed.Database.EnsureCreatedAsync();seed.Rows.AddRange(new(){TenantId="tenant-a",Id=1,Title="A course"},new(){TenantId="tenant-b",Id=1,Title="B course"});await seed.SaveChangesAsync();}
using var services=new TenantServices(options,folder,args.FirstOrDefault()=="cache");
foreach(var tenant in new[]{"tenant-a","tenant-b"})
{var file=services.FileFor(tenant,"notes.txt");Directory.CreateDirectory(Path.GetDirectoryName(file)!);await File.WriteAllTextAsync(file,tenant+" private notes");}
using var authority=new TokenAuthority();
await using var server=await ApiHarness.StartAsync(b=>
{authority.Configure(b.Services);b.Services.AddAuthorization(o=>o.AddPolicy("Tenant",p=>p.RequireAuthenticatedUser().RequireClaim("sub").RequireAssertion(c=>!string.IsNullOrWhiteSpace(c.User.FindFirst("tenant")?.Value))));},app=>
{
    app.UseAuthentication();app.UseAuthorization();
    app.MapGet("/data",(Delegate)(async(HttpContext context)=>Results.Text(await services.Read(context.User.FindFirst("tenant")!.Value,1)))).RequireAuthorization("Tenant");
    app.MapGet("/file",(Delegate)(async(HttpContext context)=>Results.Text(await File.ReadAllTextAsync(services.FileFor(context.User.FindFirst("tenant")!.Value,"notes.txt"))))).RequireAuthorization("Tenant");
    app.MapPost("/job",(Delegate)(async(HttpContext context)=>Results.Text(await services.RunJob(new(context.User.FindFirst("tenant")!.Value,context.User.FindFirst("sub")!.Value,1))))).RequireAuthorization("Tenant");
});
async Task<string> Call(string route,string subject,string tenant,bool post=false)
{
    using var request=new HttpRequestMessage(post?HttpMethod.Post:HttpMethod.Get,route);
    request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",authority.Issue(subject:subject,tenant:tenant));
    using var response=await server.Client.SendAsync(request);response.EnsureSuccessStatusCode();return await response.Content.ReadAsStringAsync();
}
Check.That((await server.Client.GetAsync("/data")).StatusCode==HttpStatusCode.Unauthorized,"anonymous request cannot select a tenant scope");
using(var missing=new HttpRequestMessage(HttpMethod.Get,"/data"))
{missing.Headers.Authorization=new AuthenticationHeaderValue("Bearer",authority.Issue(tenant:""));Check.That((await server.Client.SendAsync(missing)).StatusCode==HttpStatusCode.Forbidden,"empty tenant claim fails closed before database access");}
var alice=await Call("/data?tenant=tenant-b","alice","tenant-a");
Check.That(alice=="A course","validated tenant claim scopes the database despite a forged query parameter");
var bob=await Call("/data","bob","tenant-b");
if(args.FirstOrDefault()=="cache")Check.That(bob=="B course","incorrect expectation: an unscoped cache key isolates tenants");
Check.That(bob=="B course","cache key and EF filter both preserve tenant B's result");
Check.That(await Call("/file","alice","tenant-a")=="tenant-a private notes" && await Call("/file","bob","tenant-b")=="tenant-b private notes","tenant-derived file namespaces keep the same file name isolated");
bool traversal=false;try{services.FileFor("tenant-a","../notes.txt");}catch(UnauthorizedAccessException){traversal=true;}
Check.That(traversal,"file identifiers cannot escape their tenant namespace");
Check.That(await Call("/job","alice","tenant-a",true)=="A course" && await Call("/job","bob","tenant-b",true)=="B course","background job explicitly rehydrates its tenant database scope");
bool tampered=false;try{await services.RunJob(new("tenant-b","alice",1));}catch(UnauthorizedAccessException){tampered=true;}
Check.That(tampered,"worker rejects a job whose actor and tenant disagree");
Console.WriteLine("service: database, cache, file and worker boundaries verified with signed tenant identities");
