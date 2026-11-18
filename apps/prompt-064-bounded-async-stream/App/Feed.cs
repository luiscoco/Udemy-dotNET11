using System.Runtime.CompilerServices;
using System.Threading.Channels;
public sealed class Feed
{
    public int Produced, PeakBuffered, Stopped;
    public async IAsyncEnumerable<int> ReadAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
        var queue = Channel.CreateBounded<int>(new BoundedChannelOptions(2) { FullMode = BoundedChannelFullMode.Wait });
        Task producer = ProduceAsync(queue, lifetime.Token);
        try
        {
            await foreach (int value in queue.Reader.ReadAllAsync(token)) yield return value;
        }
        finally
        {
            lifetime.Cancel();
            try { await producer; } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        }
    }
    private async Task ProduceAsync(Channel<int> queue, CancellationToken token)
    {
        try
        {
            for (int i = 1; i <= 1000; i++)
            {
                await queue.Writer.WriteAsync(i, token);
                Produced++;
                PeakBuffered = Math.Max(PeakBuffered, queue.Reader.Count);
            }
            queue.Writer.TryComplete();
        }
        catch (Exception error) { queue.Writer.TryComplete(error); throw; }
        finally { Stopped = 1; }
    }
}
