using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text.Json;
public static class AgentRuntime
{
    public const string Model = "qwen3:0.6b";
    public static ObservedClient Client() => new(new OllamaApiClient(
        new Uri(Environment.GetEnvironmentVariable("COURSE_AI_ENDPOINT") ?? "http://127.0.0.1:11439"),Model));
    public static ChatOptions Options(string instructions, IList<AITool>? tools = null) => new()
    {
        Instructions = instructions + " /no_think", Temperature = 0,
        MaxOutputTokens = 96, Tools = tools,
        AdditionalProperties = new() { ["think"] = false }
    };
    public static AIAgent Create(ObservedClient client, string name, string instructions,
        IList<AITool>? tools = null) => client.AsAIAgent(new ChatClientAgentOptions
    {
        Id = name, Name = name, ChatOptions = Options(instructions,tools)
    });
    public static async Task<string> Ask(AIAgent agent, string input, AgentSession? session = null,
        CancellationToken cancellationToken = default)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(TimeSpan.FromSeconds(90));
        var response = await agent.RunAsync(input,session,cancellationToken:limit.Token);
        Check.That(!string.IsNullOrWhiteSpace(response.Text),"actual agent response contains text");
        Console.WriteLine("AGENT " + response.Text.Replace('\n',' ')); return response.Text;
    }
    public static void Save(object evidence) => Harness.Save("agent-evidence.json",evidence);
}
public sealed class ObservedClient(IChatClient inner) : DelegatingChatClient(inner)
{
    public int Calls { get; private set; }
    public List<ChatMessage> LastMessages { get; private set; } = [];
    public string InputText => string.Join("\n",LastMessages.Select(m=>m.Text));
    public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Calls++; LastMessages = messages.ToList();
        return base.GetResponseAsync(LastMessages,options,cancellationToken);
    }
    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Calls++; LastMessages = messages.ToList();
        return base.GetStreamingResponseAsync(LastMessages,options,cancellationToken);
    }
}
