using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using var client = AgentRuntime.Client(); int toolCalls = 0;
var lookup = AIFunctionFactory.Create(([Description("Course code")] string code) =>
{
    toolCalls++; return code == "NET11" ? "Seven runnable labs" : "Unknown course";
},"GetCourse","Read the local synthetic course catalog. No side effects.");
AIAgent agent = AgentRuntime.Create(client,"CourseTutor",
    "Always use GetCourse before answering course catalog questions. Answer in one sentence.",[lookup]);
AgentSession session = await agent.CreateSessionAsync();
int updates = 0; string answer = "";
using (var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90)))
    await foreach (var update in agent.RunStreamingAsync("Use GetCourse for code NET11.",session,
        cancellationToken:deadline.Token))
    {
        if (string.IsNullOrEmpty(update.Text)) continue;
        updates++; answer += update.Text; Console.Write(update.Text);
    }
Console.WriteLine($"\nSTREAM updates={updates} toolCalls={toolCalls}");
Check.That(updates > 1 && answer.Length > 0 && toolCalls == 1,
    "real streaming completes after exactly one read-only tool call");
bool cancelled = await CancellationProbe.Verify(agent,args.Contains("problem"));
Console.WriteLine($"CANCEL observed={cancelled}");
Check.That(cancelled,"active enumeration observes cancellation","AGENT cancellation was ignored");
AgentRuntime.Save(new { passed=true, updates, toolCalls, answer, cancelled, client.Calls });
