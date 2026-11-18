using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
if(args.Contains("stdio"))
{
    var builder=McpSession.ServerBuilder();
    builder.Services.AddSingleton(new ProbeState(args[1],args.Contains("ignore-cancel"),args[^1]));
    builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<ProbeTools>();
    await builder.Build().RunAsync();return;
}
string folder=Path.GetFullPath(Path.Combine("artifacts","cancel-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(45));
await using var client=await McpSession.Stdio([folder,args.Contains("problem")?"ignore-cancel":"safe",Guid.NewGuid().ToString("N")],deadline.Token);
var tool=(await client.ListToolsAsync()).Single(t=>t.Name=="search_catalog");
var properties=tool.JsonSchema.GetProperty("properties");
Check.That(properties.GetProperty("limit").GetProperty("type").GetString()=="integer"&&properties.GetProperty("query").GetProperty("type").GetString()=="string",
    "actual discovery exposes typed generated argument schemas");
var result=await client.CallToolAsync("search_catalog",McpSession.Args(("query","NET11"),("limit",2)));
Check.That(result.IsError!=true&&McpSession.Data(result).GetArrayLength()==2,"a valid call returns the requested bounded result count");
var invalid=await client.CallToolAsync("search_catalog",McpSession.Args(("query","NET11"),("limit",99)));
Check.That(invalid.IsError==true&&McpSession.Text(invalid).Contains("limit 1 to 3"),"domain errors return safe actionable tool-error content");
bool wrongType=false;
try { wrongType=(await client.CallToolAsync("search_catalog",McpSession.Args(("query","NET11"),("limit","not-a-number")))).IsError==true; }
catch(ModelContextProtocol.McpException){wrongType=true;}
Check.That(wrongType,"argument binding rejects an incompatible JSON type");
var cancelled=await CancellationCheck.Run(client,folder);
Check.That(cancelled.ClientCancelled&&cancelled.ServerCancelled&&!cancelled.Completed,
    "both client and actual server observe cancellation","MCP server ignored cancellation");
Check.That(McpSession.Text(await client.CallToolAsync("ping"))=="ready","the connection remains usable after cancellation");
McpSession.Save(new {passed=true,realStdio=true,schemaTypes=true,validResults=2,domainIsError=invalid.IsError,wrongType,clientCancelled=cancelled.ClientCancelled,serverCancelled=cancelled.ServerCancelled,completedAfterCancel=cancelled.Completed});
