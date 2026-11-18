using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using System.Net.Http.Headers;
bool unsafePermissions=args.Contains("problem");
var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services,unsafePermissions);
var ledger=new StockLedger(unsafePermissions);builder.Services.AddSingleton(ledger);
builder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless)
    .AddAuthorizationFilters().WithTools<StockTools>();
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();app.MapMcp("/mcp").RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);
using var http=server.Client(tokens.Issue(role:"operator",write:true));
await using var client=await McpSession.Http(server.McpUri,http);
var operatorTools=await client.ListToolsAsync();
Check.That(operatorTools.Any(t=>t.Name=="reserve_stock"),"an authorized operator discovers its write tool");
// The fixture represents a role change by replacing the signed token on subsequent requests.
http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Issue(role:"reader"));
bool rejected=false;
try
{
    var result=await client.CallToolAsync("reserve_stock",McpSession.Args(("quantity",1),("requestId","stale-list-7")));
    rejected=result.IsError==true;
}
catch(Exception) { rejected=true; }
var refreshed=await client.ListToolsAsync();bool hidden=!refreshed.Any(t=>t.Name=="reserve_stock");
Console.WriteLine($"MCP staleListDenied={rejected} hiddenAfterRefresh={hidden} mutations={ledger.Store.Effects}");
Check.That(rejected&&hidden&&ledger.Store.Effects==0,"current permissions override a cached tool list","MCP stale tool list bypassed current permissions");
var read=await client.CallToolAsync("read_stock",McpSession.Args(("sku","NET11")));
Check.That(read.IsError!=true,"the downgraded user retains its authorized read capability");
using var anonymous=server.Client();int missing=await AuthProbe.Initialize(anonymous);
Check.That(missing==401,"the endpoint independently requires authenticated access");
var schema=refreshed.Single(t=>t.Name=="read_stock").JsonSchema.GetProperty("properties");
Check.That(!schema.TryGetProperty("user",out _),"trusted principal identity is not a model-supplied argument");
http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Issue(role:"operator",write:true));
var approved=await client.CallToolAsync("reserve_stock",McpSession.Args(("quantity",1),("requestId","approved-7")));
Check.That(approved.IsError!=true&&ledger.Store.Effects==1,"an authorized operator can perform the permitted local mutation");
http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Issue(role:"reader"));
int remaining=McpSession.Data(await client.CallToolAsync("read_stock",McpSession.Args(("sku","NET11")))).GetProperty("available").GetInt32();
Check.That(remaining==9,"authorized reads reflect the actual protected business update");
McpSession.Save(new {passed=true,actualHttp=true,rejected,hidden,unauthorizedMutations=0,authorizedMutations=ledger.Store.Effects,remaining,missing,principalExcludedFromSchema=true,roleChangeOnEveryRequest=true});
