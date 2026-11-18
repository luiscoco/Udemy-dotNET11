var feed = new Feed();
if (args.FirstOrDefault() == "materialize")
{
    int[] all = await feed.ReadAsync().ToArrayAsync();
    Console.WriteLine($"Materialized result count {all.Length}");
    Check.That(all.Length <= 3, "expected the consumer to retain only three requested values");
    return;
}
using var cancellation = new CancellationTokenSource();
if (args.FirstOrDefault() == "cancel") cancellation.Cancel();
var values = new List<int>();
await foreach (int value in feed.ReadAsync().WithCancellation(cancellation.Token))
{
    values.Add(value);
    await Task.Delay(1);
    if (values.Count == 3) break;
}
Check.That(values.SequenceEqual(new[] { 1, 2, 3 }), "the consumer processes three ordered values");
Check.That(feed.PeakBuffered <= 2, "the prefetch channel retains at most two queued values");
Check.That(feed.Produced <= 6 && feed.Stopped == 1, "early disposal stops and joins the bounded producer");
Console.WriteLine($"Consumed {values.Count}; produced {feed.Produced}; peak queued {feed.PeakBuffered}; producer stopped {feed.Stopped}");
