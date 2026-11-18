var feed = new Feed();
var pipeline = new Pipeline();
if (args.FirstOrDefault() == "ignored")
{
    using var cancelled = new CancellationTokenSource();
    cancelled.Cancel();
    int[] unexpected = await pipeline.Create(feed).Take(3).ToArrayAsync();
    Console.WriteLine($"Consumed values despite unforwarded cancellation {unexpected.Length}");
    Check.That(unexpected.Length == 0, "expected cancellation without forwarding its token");
    return;
}
int[] values = await pipeline.Create(feed).Take(3).ToArrayAsync(CancellationToken.None);
Check.That(values.SequenceEqual(new[] { 20, 40, 60 }) && feed.Produced == 6, "bounded query pulls six source values");
Check.That(pipeline.Peak == 1 && pipeline.InFlight == 0, "async projection keeps one operation in flight");
Check.That(feed.Readers == 0 && feed.Disposals == 1, "Take disposes the partial feed");
var cancelledFeed = new Feed();
using var cancellation = new CancellationTokenSource();
bool observed = false;
try
{
    await foreach (int value in new Pipeline().Create(cancelledFeed).WithCancellation(cancellation.Token))
        cancellation.Cancel();
}
catch (OperationCanceledException) { observed = true; }
Check.That(observed && cancelledFeed.Readers == 0 && cancelledFeed.Disposals == 1,
    "forwarded cancellation stops and disposes the feed");
Console.WriteLine($"Bounded values {string.Join(", ", values)}, peak work {pipeline.Peak}");
