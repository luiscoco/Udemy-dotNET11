using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
if(args.Contains("stdio"))
{
    var builder=McpSession.ServerBuilder();builder.Services.AddSingleton<DemoCatalog>();
    builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<DemoTools>()
        .WithResources<DemoResources>().WithPrompts<DemoPrompts>();
    await builder.Build().RunAsync();return;
}
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(45));
await using var client=await McpSession.Stdio(cancellationToken:deadline.Token);
var tools=await client.ListToolsAsync(cancellationToken:deadline.Token);
var resources=await client.ListResourcesAsync(cancellationToken:deadline.Token);
var prompts=await client.ListPromptsAsync(cancellationToken:deadline.Token);
Check.That(tools.Count==2&&resources.Count==1&&prompts.Count==1,"real discovery returns the registered MCP primitives");
var resource=await client.ReadResourceAsync(resources.Single().Uri,cancellationToken:deadline.Token);
var prompt=await client.GetPromptAsync("review_policy",McpSession.Args(("documentId","retention")),cancellationToken:deadline.Token);
Check.That(resource.Contents.OfType<TextResourceContents>().Single().Text.Contains("seven")&&prompt.Messages.Count==2,
    "resource data and reusable prompt messages are distinct protocol results");
string? answer=null;
if(!args.Contains("problem"))answer=McpSession.Text(await client.CallToolAsync("lookup_course",McpSession.Args(("code","NET11")),cancellationToken:deadline.Token));
int calls=int.Parse(McpSession.Text(await client.CallToolAsync("read_counts",cancellationToken:deadline.Token)));
Console.WriteLine($"MCP protocol={client.NegotiatedProtocolVersion} tools={tools.Count} resources={resources.Count} prompts={prompts.Count} actualLookups={calls}");
Check.That(calls==1&&answer?.Contains("NET11")==true,"a prompt request does not substitute for actual tool invocation","MCP prompt was mistaken for tool execution");
McpSession.Save(new {passed=true,protocol=client.NegotiatedProtocolVersion,tools=tools.Count,resources=resources.Count,prompts=prompts.Count,actualLookups=calls,answer,realStdio=true});
