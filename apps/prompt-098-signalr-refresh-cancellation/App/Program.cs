using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
var issuer=new TokenIssuer();var probe=new InvocationProbe();string token=issuer.Issue("alice","reader");
await using var api=await ApiHarness.StartAsync(b=>{issuer.Configure(b);b.Services.AddSingleton(probe);b.Services.AddSignalR();},app=>
{
    app.UseAuthentication();app.UseAuthorization();app.MapHub<RefreshHub>("/refresh-hub",o=>
    {o.EnableAuthenticationRefresh=true;o.CloseOnAuthenticationExpiration=true;});
});
await using var connection=new HubConnectionBuilder().WithUrl(new Uri(api.Client.BaseAddress!,"refresh-hub"),o=>o.AccessTokenProvider=()=>Task.FromResult<string?>(token))
    .WithAuthenticationRefresh(o=>o.EnableAutoRefresh=false).Build();
int refreshed=0;connection.AuthenticationRefreshed+=_=>{Interlocked.Increment(ref refreshed);return Task.CompletedTask;};
await connection.StartAsync();string? originalId=connection.ConnectionId;
Check.That(await connection.InvokeAsync<string>("ReadState")=="1","the original connection starts with revision-one claims");
token=issuer.Issue("alice","editor","2");
if(args.FirstOrDefault()=="stale")Check.That(await connection.InvokeAsync<string>("ReadState")=="2","changing the provider token alone does not refresh the cached connection principal");
await connection.RefreshAuthenticationAsync();
Check.That(connection.ConnectionId==originalId && await connection.InvokeAsync<string>("ReadState")=="2" && refreshed==1,"explicit refresh updates claims while preserving the connection identifier");
Check.That(await connection.InvokeAsync<string>("Edit")=="editor allowed","later invocations authorize against the refreshed role");
token=issuer.Issue("bob","editor","3");bool rejected=false;
try{await connection.RefreshAuthenticationAsync();}catch(HttpRequestException){rejected=true;}
Check.That(rejected && await connection.InvokeAsync<string>("ReadState")=="2","a refresh cannot switch the connection to another user identity");token=issuer.Issue("alice","editor","2");
using var cancellation=new CancellationTokenSource();Task work=connection.InvokeAsync("LongWork",cancellation.Token);
await probe.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));cancellation.Cancel();
try{await work;throw new Exception("Expected client cancellation");}catch(OperationCanceledException){}
await probe.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check.That(connection.State==HubConnectionState.Connected && await connection.InvokeAsync<string>("ReadState")=="2","ordinary invocation cancellation reaches the server while the connection remains usable");
Console.WriteLine("network: same connection after refresh; different-user refresh rejected; server-side ordinary invocation cancellation verified");
