using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.ComponentModel;
public sealed class DemoCatalog
{
    public int Lookups { get; private set; }
    public string Read(string code) { Lookups++;return code=="NET11"?"Synthetic NET11 catalog: seven labs.":"Unknown course"; }
}
[McpServerToolType]
public sealed class DemoTools
{
    [McpServerTool(Name="lookup_course"),Description("Read the authored synthetic course catalog.")]
    public static string Lookup(string code,DemoCatalog catalog)=>catalog.Read(code);
    [McpServerTool(Name="read_counts"),Description("Inspect the fixture lookup counter.")]
    public static int Count(DemoCatalog catalog)=>catalog.Lookups;
}
[McpServerResourceType]
public sealed class DemoResources
{
    [McpServerResource(UriTemplate="course://policies/retention",Name="Retention Policy",MimeType="text/plain")]
    [Description("Synthetic retention policy, version 1.")]
    public static string Read()=>"Retention is seven days.";
}
[McpServerPromptType]
public sealed class DemoPrompts
{
    [McpServerPrompt(Name="review_policy"),Description("Create a reusable policy review request.")]
    public static IEnumerable<PromptMessage> Review(string documentId)
    {
        if(documentId!="retention")throw new ModelContextProtocol.McpException("Unknown policy identifier.");
        return [new() { Role=Role.User,Content=new TextContentBlock {Text="Review the following policy facts."} },
            new() { Role=Role.User,Content=new EmbeddedResourceBlock { Resource=new TextResourceContents
            { Uri="course://policies/retention",MimeType="text/plain",Text="Retention is seven days." } } }];
    }
}
