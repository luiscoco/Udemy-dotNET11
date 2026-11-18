using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
if(args.Contains("stdio"))
{
    var builder=McpSession.ServerBuilder();
    builder.Services.AddSingleton(new ProbeState(args[1],args.Contains("ignore-cancel"),args[^1]));
    builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<ProbeTools>();
    await builder.Build().RunAsync();return;
}
string folder=Path.GetFullPath(Path.Combine("artifacts","faults-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
string nonce=Guid.NewGuid().ToString("N");
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(60));
await using var client=await McpSession.Stdio([folder,"safe",nonce],deadline.Token);
var cancellation=await CancellationCheck.Run(client,folder);
Check.That(cancellation.ClientCancelled&&cancellation.ServerCancelled&&!cancellation.Completed,"the fault matrix verifies cooperative server cancellation");
bool disconnected=false;
try { await client.CallToolAsync("disconnect_fixture",McpSession.Args(("nonce",nonce)),cancellationToken:deadline.Token); }
catch(Exception e)when(e is not OperationCanceledException)
{
    disconnected=File.Exists(Path.Combine(folder,"disconnecting.txt"))&&
        System.Text.RegularExpressions.Regex.IsMatch(e.ToString(),@"exit code:\s*41");
    Console.WriteLine("FAULT disconnectType="+e.GetType().Name+" ownedExit41="+disconnected);
}
Check.That(disconnected,"an actual owned server exit is observed as a disconnected transport");
await using var fresh=await McpSession.Stdio([folder,"safe",nonce],deadline.Token);
Check.That(McpSession.Text(await fresh.CallToolAsync("ping"))=="ready","a new client reconnects to a fresh real server process");
var httpBuilder=WebFixture.Builder();httpBuilder.Services.AddSingleton(new ProbeState(folder,false,nonce));
httpBuilder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless).WithTools<ProbeTools>();
var app=httpBuilder.Build();app.MapMcp("/mcp");await using var server=await WebFixture.Start(app);using var raw=server.Client();
var malformed=await RawProtocol.Post(raw,"{invalid-json");
Check.That(malformed.Status==400||RawProtocol.ErrorCode(malformed.Body)==-32700,"malformed JSON is rejected over the real HTTP transport");
var unsupported=await RawProtocol.Post(raw,"{\"jsonrpc\":\"2.0\",\"id\":7,\"method\":\"resources/list\",\"params\":{}}");
int? code=RawProtocol.ErrorCode(unsupported.Body);
Check.That(code==-32601,"the actual server rejects an unsupported protocol method");
bool skipped=fresh.ServerCapabilities.Resources==null&&!args.Contains("problem");
if(!skipped)
{
    try { await fresh.ListResourcesAsync(cancellationToken:deadline.Token); }
    catch(Exception e){throw new InvalidOperationException("MCP unsupported capability was invoked",e);}
}
Console.WriteLine($"FAULT malformedStatus={malformed.Status} unsupportedCode={code} disconnected={disconnected} capabilitySkipped={skipped}");
Check.That(skipped,"the client gates optional operations on negotiated capabilities","MCP unsupported capability was invoked");
McpSession.Save(new {passed=true,malformedStatus=malformed.Status,unsupportedCode=code,disconnected,ownedDisconnectExit=41,reconnected=true,clientCancelled=cancellation.ClientCancelled,serverCancelled=cancellation.ServerCancelled,skipped,actualHttp=true,realStdio=true});
