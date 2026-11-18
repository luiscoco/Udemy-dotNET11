using System.Threading.Channels;
public sealed record DrainReport(int Accepted, int Completed, int Abandoned);
public sealed class DrainQueue : IAsyncDisposable
{
    private readonly Channel<int> queue = Channel.CreateBounded<int>(4);
    private readonly CancellationTokenSource abort = new();
    private readonly Task consumer;
    private readonly Task release;
    private int accepted, completed, abandoned;
    private readonly object admission = new();
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public DrainQueue(Task release) { this.release = release; consumer = ConsumeAsync(); }
    public bool TryEnqueue(int value)
    {
        lock (admission)
        {
            if (!queue.Writer.TryWrite(value)) return false;
            accepted++;
            return true;
        }
    }
    private async Task ConsumeAsync()
    {
        bool holding = false;
        try
        {
            await foreach (int value in queue.Reader.ReadAllAsync(abort.Token))
            {
                holding = true;
                Entered.TrySetResult();
                await release.WaitAsync(abort.Token); // Explicit fixture work gate.
                await Task.Delay(1, abort.Token);
                completed++;
                holding = false;
            }
        }
        catch (OperationCanceledException) when (abort.IsCancellationRequested)
        {
            if (holding) abandoned++;
        }
        finally { while (queue.Reader.TryRead(out _)) abandoned++; }
    }
    public async Task<DrainReport> StopAsync(TimeSpan budget)
    {
        lock (admission) queue.Writer.TryComplete();
        using var hardDeadline = new CancellationTokenSource(budget);
        using var drainWindow = new CancellationTokenSource(budget / 2);
        try { await consumer.WaitAsync(drainWindow.Token); }
        catch (OperationCanceledException) when (drainWindow.IsCancellationRequested)
        {
            abort.Cancel();
            await consumer.WaitAsync(hardDeadline.Token);
        }
        return new(accepted, completed, abandoned);
    }
    public async ValueTask DisposeAsync()
    {
        lock (admission) queue.Writer.TryComplete();
        abort.Cancel();
        await consumer;
        abort.Dispose();
    }
}
