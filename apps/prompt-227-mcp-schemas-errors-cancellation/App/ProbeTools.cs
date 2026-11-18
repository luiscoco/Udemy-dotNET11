using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
public sealed class ProbeState(string folder,bool ignoreCancellation,string nonce)
{
    public string Folder { get; }=folder;
    public bool IgnoreCancellation { get; }=ignoreCancellation;
    public string Nonce { get; }=nonce;
    public string Marker(string name)=>Path.Combine(Folder,name+".txt");
}
[McpServerToolType]
public sealed class ProbeTools
{
    [McpServerTool(Name="ping",ReadOnly=true)]
    public static string Ping()=>"ready";
    [McpServerTool(Name="search_catalog",ReadOnly=true),Description("Search the synthetic catalog with bounded query and result count.")]
    public static string[] Search([Description("Query from 1 to 40 characters")]string query,
        [Description("Result limit from 1 to 3")]int limit)
    {
        if(string.IsNullOrWhiteSpace(query)||query.Length>40||limit is <1 or >3)
            throw new McpException("Use a query of 1 to 40 characters and limit 1 to 3.");
        return new[]{"NET11 MCP catalog","NET11 cancellation guide","NET11 streaming guide"}
            .Where(s=>s.Contains(query,StringComparison.OrdinalIgnoreCase)).Take(limit).ToArray();
    }
    [McpServerTool(Name="slow_probe",ReadOnly=true),Description("Cooperative local delay used only for transport testing.")]
    public static async Task<string> Slow(int milliseconds,ProbeState state,CancellationToken cancellationToken)
    {
        if(milliseconds is <100 or >5000)throw new McpException("Delay must be 100 to 5000 milliseconds.");
        File.WriteAllText(state.Marker("started"),"started");
        try
        {
            await Task.Delay(milliseconds,state.IgnoreCancellation?CancellationToken.None:cancellationToken);
            File.WriteAllText(state.Marker("completed"),"completed");return "completed";
        }
        catch(OperationCanceledException)
        {
            File.WriteAllText(state.Marker("cancelled"),"cancelled");throw;
        }
    }
    [McpServerTool(Name="disconnect_fixture"),Description("Terminate only this owned test server after checking its launch nonce.")]
    public static async Task<string> Disconnect(string nonce,ProbeState state)
    {
        if(nonce!=state.Nonce)throw new McpException("Invalid fixture nonce.");
        await Task.Delay(50);Environment.Exit(41);return "unreachable";
    }
}
