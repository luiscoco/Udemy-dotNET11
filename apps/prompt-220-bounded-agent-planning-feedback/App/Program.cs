if(args.Contains("problem"))
{
    var unsafeLoop=await PlanningLoop.Run(noProgress:true,ignoreBudget:true);
    Console.WriteLine($"PLAN controlled status={unsafeLoop.Status} steps={unsafeLoop.Steps} budget=2");
    Check.That(unsafeLoop.Steps<=2,"planning respects the shared step ceiling","AGENT planning exceeded step budget");
    return;
}
var completed=await PlanningLoop.Run(noProgress:false,ignoreBudget:false);
Check.That(completed.Status=="Completed"&&completed.Value==2&&completed.Steps<=2,"deterministic evidence establishes completion inside budget");
var exhausted=await PlanningLoop.Run(noProgress:true,ignoreBudget:false);
Check.That(exhausted.Status=="BudgetExhausted"&&exhausted.Steps==2&&exhausted.ModelCalls==2&&exhausted.ToolCalls==2,
    "no-progress feedback terminates at the shared model and tool budget");
using var cancelled=new CancellationTokenSource(); cancelled.Cancel();
var cancelledState=await PlanningLoop.Run(false,false,cancelled.Token);
Check.That(cancelledState.Status=="Cancelled"&&cancelledState.ModelCalls==0,"cancelled state is visible without model work");
Console.WriteLine($"PLAN completed={completed.Status} blocked={exhausted.Status} cancelled={cancelledState.Status}");
AgentRuntime.Save(new { passed=true,budget=2,completed,exhausted,cancelled=cancelledState,outputTokensPerCall=48,deadlineSeconds=90 });
