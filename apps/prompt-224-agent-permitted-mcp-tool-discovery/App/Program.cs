using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services);
var ledger=new StockLedger();builder.Services.AddSingleton(ledger);
builder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless)
    .AddAuthorizationFilters().WithTools<StockTools>();
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();app.MapMcp("/mcp").RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);using var http=server.Client(tokens.Issue());
await using var mcp=await McpSession.Http(server.McpUri,http);
var discovered=await mcp.ListToolsAsync();
Check.That(discovered.Any(t=>t.Name=="read_stock")&&!discovered.Any(t=>t.Name=="reserve_stock"),
    "server discovery exposes only tools permitted to the reader");
string required=args.Contains("problem")?"wrong_tool_name":"read_stock";
var allowed=discovered.Where(t=>t.Name==required).Cast<AITool>().ToList();
Console.WriteLine($"MCP requiredTool={required} allowlisted={allowed.Count} beforeModelCalls=0");
Check.That(allowed.Count==1,"the host requires its exact approved tool before invoking a model","MCP required allowlisted tool is missing");
using var chat=AgentRuntime.Client();
AIAgent agent=AgentRuntime.Create(chat,"StockTutor","Use read_stock once to answer stock questions. Never invent stock and never make purchases.",allowed);
string answer=await AgentRuntime.Ask(agent,"Use read_stock to read the synthetic SKU NET11 and report availability in one sentence.");
Console.WriteLine($"MCP discovered={discovered.Count} allowlisted={allowed.Count} actualRemoteReads={ledger.Reads} mutations={ledger.Store.Effects}");
Check.That(allowed.Count==1&&ledger.Reads==1,"the actual agent invokes the allowlisted remote MCP tool","MCP agent did not invoke the allowlisted tool");
Check.That(ledger.Store.Effects==0,"the reader agent performs no business mutation");
McpSession.Save(new {passed=true,actualHttp=true,actualAgent=true,discovered=discovered.Select(t=>t.Name),allowlisted=allowed.Count,remoteReads=ledger.Reads,mutations=ledger.Store.Effects,modelCalls=chat.Calls,answer});
