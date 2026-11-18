using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
var issuer=new TokenIssuer();string prefix="course97-"+Guid.NewGuid().ToString("N");
string redis=Environment.GetEnvironmentVariable("COURSE_REDIS")??"127.0.0.1:6397";
using var backplane=await ConnectionMultiplexer.ConnectAsync(redis+",abortConnect=true,connectTimeout=10000");await backplane.GetDatabase().PingAsync();
void Configure(WebApplicationBuilder builder)
{
    issuer.Configure(builder);builder.Services.AddSignalR().AddStackExchangeRedis(redis,o=>o.Configuration.ChannelPrefix=RedisChannel.Literal(prefix));
}
void Map(WebApplication app) {app.UseAuthentication();app.UseAuthorization();app.MapHub<CourseHub>("/course");}
await using var nodeA=await ApiHarness.StartAsync(Configure,Map);await using var nodeB=await ApiHarness.StartAsync(Configure,Map);
HubConnection Connect(Uri address,string token)=>new HubConnectionBuilder().WithUrl(new Uri(address,"course"),o=>o.AccessTokenProvider=()=>Task.FromResult<string?>(token)).Build();
await using var alice=Connect(nodeA.Client.BaseAddress!,issuer.Issue("alice","instructor"));
await using var bob=Connect(nodeB.Client.BaseAddress!,issuer.Issue("bob","student"));
await using var eve=Connect(nodeB.Client.BaseAddress!,issuer.Issue("eve","student",course:"other"));
TaskCompletionSource<string> first=new(TaskCreationOptions.RunContinuationsAsynchronously),second=new(TaskCreationOptions.RunContinuationsAsynchronously);
alice.On<string>("Notice",message=>first.TrySetResult(message));bob.On<string>("Notice",message=>second.TrySetResult(message));
await Task.WhenAll(alice.StartAsync(),bob.StartAsync(),eve.StartAsync());
await alice.InvokeAsync("Join","dotnet");await bob.InvokeAsync("Join","dotnet");
bool denied=false;try{await eve.InvokeAsync("Join","dotnet");}catch(HubException){denied=true;}
Check.That(denied,"an authenticated user without room access cannot join the group");
await alice.InvokeAsync("Publish","dotnet","Lesson ready");
Check.That(await first.Task.WaitAsync(TimeSpan.FromSeconds(10))=="Lesson ready" && await second.Task.WaitAsync(TimeSpan.FromSeconds(10))=="Lesson ready","the Redis backplane delivers one authorized group message across two independent hosts");
bool publishDenied=false;try{await bob.InvokeAsync("Publish","dotnet","Not permitted");}catch(HubException){publishDenied=true;}
Console.WriteLine("network: two Kestrel nodes; Redis group delivery verified; unauthorized join and publish rejected");
if(args.FirstOrDefault()=="permission")Check.That(!publishDenied,"group membership must not grant instructor publishing permission");
Check.That(publishDenied,"the publishing method enforces its role requirement separately from group membership");
using var anonymous=await nodeA.Client.PostAsync("/course/negotiate?negotiateVersion=1",null);
Check.That((int)anonymous.StatusCode==401,"the hub negotiation endpoint rejects an unauthenticated client");
