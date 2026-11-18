using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
await using var dns=new DnsFixture();
#pragma warning disable SYSLIB5008 // Explicit evaluation of experimental per-connection eviction.
await using var resolver=new DnsResolver(new DnsResolverOptions{Servers=[dns.Endpoint]});
var txt=await resolver.ResolveTxtAsync(Guid.NewGuid().ToString("N")+".course.test");
Check.That(txt.ResponseCode==DnsResponseCode.NoError&&txt.Records.Single().Values.Single()=="course-v1"&&txt.Records.Single().Ttl.TotalSeconds>0&&txt.Records.Single().Ttl.TotalSeconds<=30,"actual typed DNS returns fixture TXT data and bounded TTL");
var builder=WebApplication.CreateSlimBuilder();builder.WebHost.UseUrls("http://127.0.0.1:0");
await using var app=builder.Build();app.MapGet("/id",(Microsoft.AspNetCore.Http.HttpContext c)=>c.Connection.Id);await app.StartAsync();
try{
    string address=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    int generation=0,callbacks=0;
    using var handler=new SocketsHttpHandler{PooledConnectionIdleTimeout=TimeSpan.FromSeconds(8),ShouldEvictConnection=(context,token)=>{Interlocked.Increment(ref callbacks);return Task.FromResult(generation>0&&!args.Contains("problem"));}};
    using var client=new HttpClient(handler);string first=await client.GetStringAsync(address+"/id");string reused=await client.GetStringAsync(address+"/id");
    Check.That(first==reused,"the actual HTTP connection is reused before invalidation");
    int before=Volatile.Read(ref callbacks);generation=1;
    for(int poll=0;poll<60&&Volatile.Read(ref callbacks)==before;poll++)await Task.Delay(100);
    await Task.Delay(200);string replacement=await client.GetStringAsync(address+"/id");
    Console.WriteLine($"DNS txt={txt.Records[0].Values[0]} ttl={txt.Records[0].Ttl.TotalSeconds} callbacks={callbacks} before={first} after={replacement}");
    Check.That(first!=replacement&&callbacks>0,"the actual callback retired the invalidated connection","PREVIEW stale HTTP connection was retained");
    Harness.Save("preview-evidence.json",new{passed=true,actualTypedDns=true,txt="course-v1",ttl=txt.Records[0].Ttl.TotalSeconds,actualHttp=true,reused=first==reused,evicted=first!=replacement,callbacks,os=System.Runtime.InteropServices.RuntimeInformation.OSDescription});
}finally{await app.StopAsync();}
