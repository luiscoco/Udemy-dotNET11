using System.Collections.Concurrent;
using System.Diagnostics;
if (args.FirstOrDefault() == "unbounded")
{
    var workload = new Workload();
    var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    Task<int>[] tasks = Enumerable.Range(1, 12).Select(i => workload.ProcessAsync(i, CancellationToken.None, gate.Task)).ToArray();
    int peak = workload.Peak;
    gate.SetResult();
    await Task.WhenAll(tasks);
    Console.WriteLine($"Unbounded launch: peak {peak}; peak buffer bytes {workload.PeakBytes}");
    Check.That(peak <= 3, "starting every task must not exceed the chosen concurrency budget");
    return;
}
Directory.CreateDirectory("artifacts");
var rows = new List<string> { "degree,elapsedMs,itemsPerSecond,peakActive,peakBufferBytes" };
foreach (int degree in new[] { 1, 3 })
{
    var workload = new Workload();
    var values = new ConcurrentBag<int>();
    var watch = Stopwatch.StartNew();
    await Parallel.ForEachAsync(Enumerable.Range(1, 12),
        new ParallelOptions { MaxDegreeOfParallelism = degree },
        async (value, token) => values.Add(await workload.ProcessAsync(value, token)));
    watch.Stop();
    Check.That(values.Order().SequenceEqual(Enumerable.Range(1, 12).Select(i => i * 2)), $"degree {degree} retains every result");
    Check.That(workload.Peak <= degree && workload.PeakBytes <= degree * 4096, $"degree {degree} bounds in-flight work and owned payload buffers");
    string row = FormattableString.Invariant($"{degree},{watch.Elapsed.TotalMilliseconds:F2},{12 / watch.Elapsed.TotalSeconds:F2},{workload.Peak},{workload.PeakBytes}");
    rows.Add(row);
    Console.WriteLine(row);
}
await File.WriteAllLinesAsync("artifacts/throughput.csv", rows);
Console.WriteLine("Elapsed time is observed, not a speed assertion. Peak buffer ownership is not total GC or process memory.");
