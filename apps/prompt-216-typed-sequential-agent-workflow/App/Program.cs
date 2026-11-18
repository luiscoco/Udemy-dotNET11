using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using var client=AgentRuntime.Client();
var options=AgentRuntime.Options("Explain only the supplied quote. Return JSON with Total, Currency and a short Summary. Do not change the facts.");
options.ResponseFormat=ChatResponseFormat.ForJsonSchema(QuoteContract.Schema);
AIAgent writer=client.AsAIAgent(new ChatClientAgentOptions { Name="QuoteWriter",ChatOptions=options });
var calculate=new CalculateQuote(); var explain=new ExplainQuote(writer); var validate=new ValidateQuote(args.Contains("problem"));
var workflow=new WorkflowBuilder(calculate).AddEdge(calculate,explain).AddEdge(explain,validate)
    .WithOutputFrom(validate).Build();
ReviewedQuote? output=null; var stages=new List<string>();
using var limit=new CancellationTokenSource(TimeSpan.FromSeconds(90));
await using var run=await InProcessExecution.RunStreamingAsync(workflow,2,cancellationToken:limit.Token);
await foreach(var evt in run.WatchStreamAsync())
{
    if(evt is ExecutorCompletedEvent stage) { stages.Add(stage.ExecutorId); Console.WriteLine("WORKFLOW completed="+stage.ExecutorId); }
    if(evt is WorkflowOutputEvent result) output=(ReviewedQuote)result.Data!;
    if(evt is ExecutorFailedEvent failure) throw new InvalidOperationException("AGENT workflow accepted unsupported total: "+failure.Data);
    if(evt is WorkflowErrorEvent error) throw new InvalidOperationException("Workflow failed",error.Exception);
}
Check.That(output?.Facts.Total==18m&&output.Draft.Total==18m,"validated workflow output matches deterministic calculation");
Check.That(stages.SequenceEqual(new[]{"calculate","explain","validate"}),"typed stages execute in their declared order");
Console.WriteLine($"QUOTE quantity=2 total={output!.Facts.Total:F2} currency={output.Facts.Currency}");
Harness.Save("quote-draft.json",output);
AgentRuntime.Save(new { passed=true,stages,total=output.Facts.Total,frameworkWorkflow=true,output });
