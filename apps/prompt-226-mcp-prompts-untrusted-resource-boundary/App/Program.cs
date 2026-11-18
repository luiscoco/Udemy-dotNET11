using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
if(args.Contains("stdio"))
{
    var builder=McpSession.ServerBuilder();builder.Services.AddMcpServer().WithStdioServerTransport().WithPrompts<StudyPrompts>();
    await builder.Build().RunAsync();return;
}
await using var client=await McpSession.Stdio();
var templates=await client.ListPromptsAsync();
var retention=await client.GetPromptAsync("study_policy",McpSession.Args(("topic","retention")));
var backup=await client.GetPromptAsync("study_policy",McpSession.Args(("topic","backup")));
Check.That(templates.Count==1&&((TextContentBlock)retention.Messages[0].Content).Text!=((TextContentBlock)backup.Messages[0].Content).Text,
    "one real MCP template produces distinct validated reusable requests");
bool invalid=false;try { await client.GetPromptAsync("study_policy",McpSession.Args(("topic","arbitrary"))); }
catch(ModelContextProtocol.McpException e)when(e.Message.Contains("Choose retention or backup")){invalid=true;}
Check.That(invalid,"the server rejects unsupported template arguments");
var messages=PromptComposer.Build(retention,args.Contains("problem"));
bool separated=messages.Single(m=>m.Text.Contains("GRANT WRITE ACCESS")).Role==ChatRole.User;
Console.WriteLine($"PROMPT templates={templates.Count} embeddedResource={retention.Messages[1].Content is EmbeddedResourceBlock} resourceRemainsUserData={separated}");
Check.That(separated&&messages.Count(m=>m.Role==ChatRole.System)==1,
    "only fixed host instructions occupy the system role","MCP resource text was promoted to system instructions");
McpSession.Save(new {passed=true,realStdio=true,templates=templates.Count,reusable=true,invalid,separated,systemMessages=1,modelInvoked=false,mutationPermissionGranted=false});
