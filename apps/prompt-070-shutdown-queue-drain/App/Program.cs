using System.Diagnostics;
var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
await using (var normal = new DrainQueue(release.Task))
{
    for (int i = 1; i <= 3; i++) Check.That(normal.TryEnqueue(i), "the open queue accepts the fixture item");
    await normal.Entered.Task;
    release.SetResult();
    DrainReport report = await normal.StopAsync(TimeSpan.FromSeconds(5));
    Check.That(report == new DrainReport(3, 3, 0), "normal shutdown drains every accepted item");
    Check.That(!normal.TryEnqueue(99), "admission remains closed after shutdown begins");
    Console.WriteLine($"Normal report: {report}");
}
var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
await using (var limited = new DrainQueue(blocked.Task))
{
    for (int i = 1; i <= 3; i++) limited.TryEnqueue(i);
    await limited.Entered.Task;
    var watch = Stopwatch.StartNew();
    DrainReport report = await limited.StopAsync(TimeSpan.FromMilliseconds(200));
    Console.WriteLine($"Deadline report: {report}; observed elapsed {watch.ElapsedMilliseconds} ms");
    if (args.FirstOrDefault() == "assume")
        Check.That(report.Completed == report.Accepted, "a finite shutdown budget cannot guarantee completion of blocked work");
    Check.That(report == new DrainReport(3, 0, 3), "the blocked fixture is cancelled and all uncompleted items are accounted for");
    Check.That(report.Accepted == report.Completed + report.Abandoned, "accepted items reconcile after the consumer has been joined");
}
