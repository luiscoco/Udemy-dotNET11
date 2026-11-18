using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
public sealed record PlannedAction(string Action,int Amount);
public sealed record LoopOutcome(string Status,int Steps,int ModelCalls,int ToolCalls,int Value,string[] Events);
public static class PlanningLoop
{
    public static async Task<LoopOutcome> Run(bool noProgress,bool ignoreBudget,CancellationToken cancellationToken=default)
    {
        using var client=AgentRuntime.Client();
        var options=AgentRuntime.Options("Choose the next step. Return JSON Action=increment and Amount=1. Only one increment per step. Do not claim completion.");
        options.MaxOutputTokens=48; options.ResponseFormat=ChatResponseFormat.ForJsonSchema<PlannedAction>();
        AIAgent planner=client.AsAIAgent(new ChatClientAgentOptions { Name="BoundedPlanner",ChatOptions=options });
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(90));
        const int budget=2,goal=2; int value=0,steps=0,tools=0; var events=new List<string>();
        try
        {
            var session=await planner.CreateSessionAsync(cancellationToken:deadline.Token);
            for(;steps<(ignoreBudget?3:budget);)
            {
                deadline.Token.ThrowIfCancellationRequested();
                var reply=await planner.RunAsync($"Goal={goal}; observed value={value}; prior feedback={events.LastOrDefault()??"none"}. Choose increment by one.",session,cancellationToken:deadline.Token);
                var plan=JsonSerializer.Deserialize<PlannedAction>(reply.Text,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!;
                if(plan.Action!="increment"||plan.Amount!=1) return new("Failed",steps,client.Calls,tools,value,[..events,"invalid-plan"]);
                steps++; tools++;
                // The no-progress case is an explicit failed-tool fixture.
                if(!noProgress) value+=plan.Amount;
                events.Add($"step={steps}; observed={value}; accepted={value>=goal}");
                Console.WriteLine("PLAN "+events[^1]);
                if(value>=goal) return new("Completed",steps,client.Calls,tools,value,[..events]);
            }
            return new("BudgetExhausted",steps,client.Calls,tools,value,[..events]);
        }
        catch(OperationCanceledException) { return new("Cancelled",steps,client.Calls,tools,value,[..events]); }
    }
}
