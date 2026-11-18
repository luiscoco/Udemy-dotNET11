var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
int started = 0;
string[] names = ["inventory", "pricing", "shipping"];
Task<int>[] tasks = names.Select(name => Operations.RunAsync(name, release.Task,
    () => Interlocked.Increment(ref started))).ToArray();
Check.That(started == 3 && tasks.All(task => !task.IsCompleted), "all independent operations start before the shared release");
Task all = Task.WhenAll(tasks);
release.SetResult();
if (args.FirstOrDefault() == "first")
{
    Exception? first = null;
    try { await all; } catch (Exception error) { first = error; }
    Console.WriteLine($"Await surfaced {first!.GetType().Name}; aggregate contains {all.Exception!.InnerExceptions.Count} errors");
    Check.That(all.Exception.InnerExceptions.Count == 1, "expected the first surfaced exception to be the only failure");
    return;
}
Outcome[] outcomes = await Task.WhenAll(names.Select((name, index) => Operations.ObserveAsync(name, tasks[index])));
foreach (var outcome in outcomes)
{
    if (outcome.Error is null) Console.WriteLine($"SUCCESS {outcome.Name}: {outcome.Value}");
    else Console.WriteLine($"FAILURE {outcome.Name}: {outcome.Error}");
}
try { await all; } catch { /* Each operation's full exception was retained above. */ }
Check.That(outcomes.Single(x => x.Name == "inventory").Value == 42, "the successful inventory result survives sibling failures");
Check.That(outcomes.Count(x => x.Error is not null) == 2, "both failed operations retain their own diagnostic");
Check.That(all.Exception!.InnerExceptions.Count == 2, "the aggregate also retains both faults");
Check.That(outcomes.Where(x => x.Error is not null).All(x => x.Error!.StackTrace!.Contains("RunAsync")),
    "original exception stack information is preserved");
