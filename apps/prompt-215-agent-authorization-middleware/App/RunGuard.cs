using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
public sealed class RunGuard(bool authorized,bool bypassAuthorization=false)
{
    public int Executions { get; private set; }
    public List<string> Events { get; } = [];
    public async Task<AgentResponse> Invoke(IEnumerable<ChatMessage> messages,AgentSession? session,
        AgentRunOptions? options,AIAgent inner,CancellationToken cancellationToken)
    {
        string runId=Guid.NewGuid().ToString("N")[..8];
        try
        {
            if(!authorized&&!bypassAuthorization) throw new UnauthorizedAccessException("agent.run denied");
            var input=messages.ToArray();
            if(input.Sum(m=>m.Text.Length) is 0 or > 200) throw new ArgumentException("Input must contain 1 to 200 characters");
            if(Executions>=1) throw new InvalidOperationException("Agent execution budget exhausted");
            using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(90));
            Executions++; Events.Add(runId+":started");
            var result=await inner.RunAsync(input,session,options,deadline.Token);
            Events.Add(runId+":completed"); return result;
        }
        catch(OperationCanceledException) { Events.Add(runId+":cancelled"); throw; }
        catch { Events.Add(runId+":rejected"); throw; }
    }
}
