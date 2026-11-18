using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services);
var ledger=new StockLedger();builder.Services.AddSingleton(ledger);
builder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless)
    .AddAuthorizationFilters().WithTools<StockTools>();
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();
var endpoint=app.MapMcp("/mcp");
if(!args.Contains("problem"))endpoint.RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);
using var valid=server.Client(tokens.Issue());
await using var client=await McpSession.Http(server.McpUri,valid);
var result=await client.CallToolAsync("read_stock",McpSession.Args(("sku","NET11")));
Check.That(result.IsError!=true&&McpSession.Data(result).GetProperty("available").GetInt32()==10,
    "a valid JWT principal invokes the actual HTTP MCP tool");
using var anonymous=server.Client();using var wrongKey=server.Client(tokens.Issue(wrongSignature:true));
using var wrongAudience=server.Client(tokens.Issue(audience:"other-service"));
using var expired=server.Client(tokens.Issue(expired:true));using var noScope=server.Client(tokens.Issue(read:false));
int missing=await AuthProbe.Initialize(anonymous),signature=await AuthProbe.Initialize(wrongKey),
    audience=await AuthProbe.Initialize(wrongAudience),expiry=await AuthProbe.Initialize(expired),scope=await AuthProbe.Initialize(noScope);
Console.WriteLine($"JWT missing={missing} signature={signature} audience={audience} expired={expiry} noScope={scope}");
Check.That(missing==401,"the MCP endpoint rejects anonymous initialization","MCP endpoint accepted an anonymous request");
Check.That(signature==401&&audience==401&&expiry==401&&scope==403,"signature, audience, expiry and endpoint permission are independently enforced");
McpSession.Save(new {passed=true,actualHttp=true,missing,signature,audience,expiry,scope,protocol=client.NegotiatedProtocolVersion,ledger.Reads});
