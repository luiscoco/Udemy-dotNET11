using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
public sealed class Worker(bool fatal) : BackgroundService
{
    public TaskCompletionSource Ready { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Finished { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConcurrentQueue<string> Events { get; } = new();
    public int Completed, Recovered, Cleaned;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Events.Enqueue("worker ready");
        Ready.SetResult();
        try
        {
            await Release.Task.WaitAsync(stoppingToken);
            foreach (int job in new[] { 1, 2, 3 })
            {
                try
                {
                    await Task.Delay(1, stoppingToken);
                    if (job == 2) throw new InvalidOperationException("Fatal or recoverable job 2");
                    Completed++;
                    Events.Enqueue($"completed job {job}");
                }
                catch (InvalidOperationException) when (!fatal)
                {
                    Recovered++;
                    Events.Enqueue("reported recoverable job 2; continuing");
                }
            }
        }
        finally
        {
            Cleaned = 1;
            Events.Enqueue("worker cleanup");
            Finished.TrySetResult();
        }
    }
}
