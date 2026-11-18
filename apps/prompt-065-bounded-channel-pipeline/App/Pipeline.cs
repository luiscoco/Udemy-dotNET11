using System.Collections.Concurrent;
using System.Threading.Channels;
public static class Pipeline
{
    public static async Task<int[]> RunAsync(bool failProducer, bool failConsumer = false)
    {
        var channel = Channel.CreateBounded<int>(2);
        var results = new ConcurrentBag<int>();
        using var stop = new CancellationTokenSource();
        async Task ConsumeAsync()
        {
            try
            {
                await foreach (int value in channel.Reader.ReadAllAsync(stop.Token))
                {
                    if (failConsumer && value == 5) throw new IOException("Consumer rejected item 5");
                    await Task.Delay(1, stop.Token);
                    results.Add(value * 2);
                }
            }
            catch { stop.Cancel(); throw; }
        }
        Task[] consumers = [ConsumeAsync(), ConsumeAsync()];
        Exception? failure = null;
        try
        {
            for (int i = 1; i <= 12; i++) await channel.Writer.WriteAsync(i, stop.Token);
            if (failProducer) throw new IOException("Producer input failed");
        }
        catch (Exception error) { failure = error; }
        finally { channel.Writer.TryComplete(failure); }
        await Task.WhenAll(consumers);
        return results.Order().ToArray();
    }
}
