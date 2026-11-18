using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
public static class McpSession
{
    public const string Protocol = "2025-11-25";
    public static HostApplicationBuilder ServerBuilder()
    {
        var builder=Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(o=>o.LogToStandardErrorThreshold=LogLevel.Trace);
        return builder;
    }
    public static Task<McpClient> Stdio(string[]? flags=null,CancellationToken cancellationToken=default)
    {
        string dotnet=Harness.Dotnet;
        var env=StdioClientTransportOptions.GetDefaultEnvironmentVariables();
        if(Path.IsPathRooted(dotnet))env["PATH"]=Path.GetDirectoryName(dotnet)+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH");
        Directory.CreateDirectory("artifacts");
        var transport=new StdioClientTransport(new()
        {
            Name="Owned course MCP server",Command="dotnet",WorkingDirectory=Environment.CurrentDirectory,
            Arguments=[Assembly.GetExecutingAssembly().Location,"stdio",..(flags??[])],
            InheritEnvironmentVariables=false,EnvironmentVariables=env,ShutdownTimeout=TimeSpan.FromSeconds(3),
            StandardErrorLines=line=>File.AppendAllText("artifacts/mcp-server-stderr.log",line+Environment.NewLine)
        });
        return McpClient.CreateAsync(transport,new() { ProtocolVersion=Protocol },cancellationToken:cancellationToken);
    }
    public static Task<McpClient> Http(Uri endpoint,HttpClient http,CancellationToken cancellationToken=default) =>
        McpClient.CreateAsync(new HttpClientTransport(new()
        {
            Name="Loopback HTTP MCP server",Endpoint=endpoint,TransportMode=HttpTransportMode.StreamableHttp,
            EnableStandaloneGetStream=false
        },http,ownsHttpClient:false),new() { ProtocolVersion=Protocol },cancellationToken:cancellationToken);
    public static string Text(CallToolResult result) => string.Join("\n",result.Content.OfType<TextContentBlock>().Select(c=>c.Text));
    public static JsonElement Data(CallToolResult result) => JsonSerializer.Deserialize<JsonElement>(Text(result));
    public static Dictionary<string,object?> Args(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
    public static void Save(object value)=>Harness.Save("mcp-evidence.json",value);
    public static async Task<bool> Until(Func<bool> predicate,TimeSpan timeout)
    {
        var watch=System.Diagnostics.Stopwatch.StartNew();
        while(watch.Elapsed<timeout){if(predicate())return true;await Task.Delay(20);}
        return predicate();
    }
}
