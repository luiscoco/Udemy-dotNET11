using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services);
var store=new PolicyStore(args.Contains("problem"));builder.Services.AddSingleton(store);
builder.Services.AddMcpServer().WithHttpTransport(o=>o.SessionMode=HttpServerSessionMode.Stateless)
    .AddAuthorizationFilters().WithResources<PolicyResources>();
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();app.MapMcp("/mcp").RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);using var alice=server.Client(tokens.Issue());
await using var client=await McpSession.Http(server.McpUri,alice);
var templates=await client.ListResourceTemplatesAsync();
Check.That(templates.Count==1&&templates[0].UriTemplate=="docs://policies/{tenant}/{id}"&&templates[0].MimeType=="text/markdown",
    "resource template metadata is useful without enumerating private identifiers");
var contents=(await client.ReadResourceAsync("docs://policies/a/retention")).Contents.OfType<TextResourceContents>().Single();
Check.That(contents.Text.Contains("Version: 1")&&contents.MimeType=="text/markdown","the actual authorized read returns typed URI content and version");
using var bob=server.Client(tokens.Issue(subject:"bob"));await using var other=await McpSession.Http(server.McpUri,bob);
bool denied=false;try { await other.ReadResourceAsync("docs://policies/a/retention"); }
catch(ModelContextProtocol.McpException e)when(e.Message.Contains("Not found or not permitted")) { denied=true; }
Console.WriteLine($"RESOURCE authorizedBytes={contents.Text.Length} otherOwnerDenied={denied}");
Check.That(denied,"a known URI does not bypass the document ACL","MCP resource ACL was bypassed");
bool tenantDenied=false;try { await client.ReadResourceAsync("docs://policies/b/retention"); }
catch(ModelContextProtocol.McpException e)when(e.Message.Contains("Not found or not permitted")) { tenantDenied=true; }
store.Revoked=true;
bool revoked=false;try { await client.ReadResourceAsync("docs://policies/a/retention"); }
catch(ModelContextProtocol.McpException e)when(e.Message.Contains("Not found or not permitted")) { revoked=true; }
Check.That(tenantDenied&&revoked,"every read rechecks current tenant and revocation boundaries");
McpSession.Save(new {passed=true,actualHttp=true,metadataTemplates=templates.Count,denied,tenantDenied,revoked,mime=contents.MimeType,version=1});
