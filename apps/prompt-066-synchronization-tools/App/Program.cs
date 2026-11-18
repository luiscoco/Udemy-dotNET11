if (args.FirstOrDefault() == "lost")
{
    int counter = 0, readCount = 0;
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    async Task UpdateAsync()
    {
        int snapshot = counter;
        if (Interlocked.Increment(ref readCount) == 2) release.SetResult();
        await release.Task;
        counter = snapshot + 1;
    }
    await Task.WhenAll(UpdateAsync(), UpdateAsync());
    Console.WriteLine($"Two updates from the same snapshot produced {counter}");
    Check.That(counter == 2, "an unprotected read-modify-write must not lose an update");
    return;
}
var account = new Account();
Parallel.For(0, 2000, _ => account.Reserve());
var snapshot = account.Snapshot();
Check.That(snapshot.Available == 0 && snapshot.Reserved == 1000, "lock preserves the paired account invariant");
int completed = 0;
Parallel.For(0, 10_000, _ => Interlocked.Increment(ref completed));
Check.That(completed == 10_000, "Interlocked preserves all independent counter increments");
var gate = new Gate();
await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => gate.UseAsync()));
Check.That(gate.Peak <= 2, "SemaphoreSlim bounds asynchronous resource use to two holders");
Console.WriteLine($"Reserved {snapshot.Reserved}; atomic increments {completed}; peak async holders {gate.Peak}");
