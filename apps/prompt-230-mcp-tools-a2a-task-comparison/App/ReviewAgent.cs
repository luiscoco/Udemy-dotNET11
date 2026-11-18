using A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
public sealed record ReviewDecision(string Check);
public sealed record ReviewArtifact(string Check,string[] EvidenceIds);
public sealed record ReviewControl(bool Fail);
public sealed class ReviewAgent(ReviewControl control) : IAgentHandler
{
    public static readonly string[] AllowedChecks=["Inspect input validation","Review cancellation propagation"];
    public const string RunbookId="runbook-7";
    public const string Runbook="Synthetic public runbook: validate inputs and propagate cancellation when reviewing a slow request.";
    public async Task ExecuteAsync(RequestContext context,AgentEventQueue eventQueue,CancellationToken cancellationToken)
    {
        var task=new TaskUpdater(eventQueue,context.TaskId,context.ContextId);
        await task.SubmitAsync(cancellationToken:cancellationToken);
        await task.StartWorkAsync(cancellationToken:cancellationToken);
        if(control.Fail){await task.FailAsync(cancellationToken:cancellationToken);return;}
        using var client=AgentRuntime.Client();
        var options=AgentRuntime.Options("Choose one approved review action from the supplied runbook. Do not claim an investigation or repair is complete.");
        options.MaxOutputTokens=48;
        options.ResponseFormat=ChatResponseFormat.ForJsonSchema(JsonSerializer.SerializeToElement(new
        {
            type="object",properties=new { Check=new { type="string",@enum=AllowedChecks } },
            required=new[]{"Check"},additionalProperties=false
        }));
        AIAgent specialist=client.AsAIAgent(new ChatClientAgentOptions { Name="SupportReviewer",ChatOptions=options });
        string response=await AgentRuntime.Ask(specialist,Runbook+"\nRequest: "+context.UserText,cancellationToken:cancellationToken);
        var decision=JsonSerializer.Deserialize<ReviewDecision>(response,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!;
        if(!AllowedChecks.Contains(decision.Check))throw new InvalidDataException("Unsupported specialist recommendation");
        var artifact=new ReviewArtifact(decision.Check,[RunbookId]);
        await task.AddArtifactAsync([Part.FromText(JsonSerializer.Serialize(artifact))],cancellationToken:cancellationToken);
        await task.CompleteAsync(cancellationToken:cancellationToken);
    }
    public static AgentCard Card()=>new()
    {
        Name="Synthetic support reviewer",Description="Select a validated review action using a public synthetic runbook.",Version="1.0",
        SupportedInterfaces=[new AgentInterface {Url="http://127.0.0.1:0/agent",ProtocolBinding="JSONRPC",ProtocolVersion="1.0"}],
        DefaultInputModes=["text/plain"],DefaultOutputModes=["text/plain"],
        Capabilities=new AgentCapabilities { Streaming=false,PushNotifications=false },
        Skills=[new A2A.AgentSkill {Id="review",Name="Review",Description="Recommend one approved diagnostic check.",Tags=["teaching","review"]}]
    };
}
