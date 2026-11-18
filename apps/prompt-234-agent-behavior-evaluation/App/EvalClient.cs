using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text.Json;
public sealed class EvalClient(IChatClient inner) : DelegatingChatClient(inner)
{
    public int Calls { get; private set; }
    public List<object> Exchanges { get; } = [];
    public List<ChatMessage> LastMessages { get; private set; } = [];
    public static EvalClient Open() => new(new OllamaApiClient(new Uri(
        Environment.GetEnvironmentVariable("COURSE_AI_ENDPOINT") ?? "http://127.0.0.1:11439"), "qwen3:0.6b"));
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options=null,CancellationToken cancellationToken=default)
    {
        LastMessages=messages.ToList();Calls++;
        options=options?.Clone()??new(); options.Temperature=0;options.MaxOutputTokens??=512;
        options.AdditionalProperties??=new();options.AdditionalProperties["think"]=false;
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(120));
        var response=await base.GetResponseAsync(LastMessages,options,deadline.Token);
        Exchanges.Add(new {input=LastMessages.Select(m=>new {role=m.Role.Value,m.Text}),output=response.Text,
            inputTokens=response.Usage?.InputTokenCount,outputTokens=response.Usage?.OutputTokenCount});
        return response;
    }
    public void Save() => Harness.Save("model-exchanges.json",new {model="qwen3:0.6b",calls=Calls,exchanges=Exchanges});
}
