using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
public sealed record Specialist(string Id,string Instruction,string[] Checks);
public sealed record ReviewCheck(string Check);
public sealed class SpecialistPool
{
    private int active, peak;
    public int Peak => peak;
    public ConcurrentDictionary<string,string> Results { get; } = new();
    public ConcurrentDictionary<string,string> Status { get; } = new();
    public async Task Run(Specialist[] specialists,int concurrency,CancellationToken cancellationToken)
    {
        using var slots=new SemaphoreSlim(concurrency);
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await Task.WhenAll(specialists.Select(async specialist=>
        {
            await slots.WaitAsync(linked.Token);
            try
            {
                int count=Interlocked.Increment(ref active),old;
                do { old=peak; if(count<=old) break; } while(Interlocked.CompareExchange(ref peak,count,old)!=old);
                Status[specialist.Id]="running";
                using var client=AgentRuntime.Client();
                var options=AgentRuntime.Options(specialist.Instruction+" Select one approved checklist action. Return JSON with Check. Never claim the work is already completed.");
                options.MaxOutputTokens=48;
                options.ResponseFormat=ChatResponseFormat.ForJsonSchema(JsonSerializer.SerializeToElement(new
                {
                    type="object",properties=new { Check=new { type="string",@enum=specialist.Checks } },
                    required=new[]{"Check"},additionalProperties=false
                }));
                var agent=client.AsAIAgent(new ChatClientAgentOptions { Id=specialist.Id,Name=specialist.Id,ChatOptions=options });
                var session=await agent.CreateSessionAsync(cancellationToken:linked.Token);
                string json=await AgentRuntime.Ask(agent,
                    "Choose one approved review action for a synthetic course release. Return only the requested JSON.",session,linked.Token);
                var selected=JsonSerializer.Deserialize<ReviewCheck>(json,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!;
                if(!specialist.Checks.Contains(selected.Check)) throw new InvalidDataException("Unapproved specialist review action");
                Results[specialist.Id]=selected.Check;
                Status[specialist.Id]="completed";
            }
            catch { Status[specialist.Id]="failed"; linked.Cancel(); throw; }
            finally { Interlocked.Decrement(ref active); slots.Release(); }
        }));
    }
}
