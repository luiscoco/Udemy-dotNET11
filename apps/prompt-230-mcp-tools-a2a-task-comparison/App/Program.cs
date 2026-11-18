using A2A;
using A2A.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using System.Text.Json;
var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services);
builder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless)
    .AddAuthorizationFilters().WithTools<QuoteTool>();
var card=ReviewAgent.Card();builder.Services.AddSingleton(new ReviewControl(args.Contains("problem")));
builder.Services.AddA2AAgent<ReviewAgent>(card);
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();
app.MapMcp("/mcp").RequireAuthorization("mcp.access");
app.MapA2A("/agent").RequireAuthorization("mcp.access");
app.MapWellKnownAgentCard(card).RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);card.SupportedInterfaces[0].Url=new Uri(server.BaseUri,"/agent").ToString();
using var http=server.Client(tokens.Issue());using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(90));
await using var mcp=await McpSession.Http(server.McpUri,http,deadline.Token);
var quote=McpSession.Data(await mcp.CallToolAsync("quote",McpSession.Args(("quantity",3)),cancellationToken:deadline.Token));
Check.That(quote.GetProperty("total").GetDecimal()==37.50m,"actual MCP tool returns the deterministic catalogue price");
var resolver=new A2ACardResolver(server.BaseUri,http);
var discovered=await resolver.GetAgentCardAsync(cancellationToken:deadline.Token);
var address=new Uri(discovered.SupportedInterfaces.Single().Url);
Check.That(address==new Uri(server.BaseUri,"/agent")&&discovered.SupportedInterfaces.Single().ProtocolVersion=="1.0",
    "the discovered A2A interface matches the owned endpoint and pinned protocol");
var remote=new A2AClient(address,http);
var reply=await remote.SendMessageAsync(new SendMessageRequest
{
    Message=new A2A.Message { MessageId=Guid.NewGuid().ToString("N"),Role=A2A.Role.User,
        Parts=[Part.FromText("Recommend a check for a slow synthetic support request.")] },
    Configuration=new() { ReturnImmediately=false }
},deadline.Token);
Console.WriteLine($"A2A taskState={reply.Task?.Status.State} artifacts={reply.Task?.Artifacts?.Count??0}");
Check.That(reply.Task?.Status.State==TaskState.Completed,"a specialist task must actually complete before accepting its result","MCP A2A specialist did not complete");
var completed=await remote.GetTaskAsync(new GetTaskRequest {Id=reply.Task!.Id},deadline.Token);
var artifacts=completed.Artifacts??throw new InvalidDataException("Completed specialist task has no artifacts");
string text=artifacts.Single().Parts.Single().Text!;
var result=JsonSerializer.Deserialize<ReviewArtifact>(text)!;
Check.That(completed.Status.State==TaskState.Completed&&ReviewAgent.AllowedChecks.Contains(result.Check)&&result.EvidenceIds.SequenceEqual(new[]{ReviewAgent.RunbookId}),
    "the real A2A task artifact carries an approved check and known permitted evidence");
Console.WriteLine($"MCP quote={quote.GetProperty("total").GetDecimal():F2} EUR; A2A recommendation={result.Check}; evidence={result.EvidenceIds[0]}");
McpSession.Save(new {passed=true,actualHttp=true,actualMcp=true,actualA2A=true,actualAgent=true,quote=37.50m,taskState=completed.Status.State.ToString(),artifacts=artifacts.Count,result.Check,result.EvidenceIds,mcpProtocol=McpSession.Protocol,a2aProtocol="1.0",a2aSdk="1.0.0-preview4"});
