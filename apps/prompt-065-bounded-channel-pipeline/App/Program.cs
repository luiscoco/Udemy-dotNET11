using System.Threading.Channels;
if (args.FirstOrDefault() == "drop")
{
    int dropped = 0;
    var lossy = Channel.CreateBounded<int>(new BoundedChannelOptions(2)
        { FullMode = BoundedChannelFullMode.DropWrite }, _ => dropped++);
    for (int i = 1; i <= 3; i++) Check.That(lossy.Writer.TryWrite(i), "the configured drop-mode writer accepts the attempt");
    lossy.Writer.Complete();
    int[] retained = await lossy.Reader.ReadAllAsync().ToArrayAsync();
    Console.WriteLine($"Retained {retained.Length}; dropped {dropped}");
    Check.That(retained.Length == 3, "expected a drop policy to preserve all messages");
    return;
}
if (args.FirstOrDefault() == "consumer")
{
    _ = await Pipeline.RunAsync(false, true);
    return;
}
if (args.FirstOrDefault() == "failure")
{
    _ = await Pipeline.RunAsync(true);
    return;
}
var queue = Channel.CreateBounded<int>(2);
await queue.Writer.WriteAsync(1);
await queue.Writer.WriteAsync(2);
Task third = queue.Writer.WriteAsync(3).AsTask();
Check.That(!third.IsCompleted, "the third write waits while both bounded slots are full");
int first = await queue.Reader.ReadAsync();
await third;
queue.Writer.Complete();
int[] remaining = await queue.Reader.ReadAllAsync().ToArrayAsync();
Check.That(first == 1 && remaining.SequenceEqual(new[] { 2, 3 }), "freeing one slot resumes the producer and preserves FIFO order");
int[] results = await Pipeline.RunAsync(false);
Check.That(results.SequenceEqual(Enumerable.Range(1, 12).Select(i => i * 2)), "two workers process every accepted message exactly once in this fixture");
Console.WriteLine(string.Join(", ", results));
