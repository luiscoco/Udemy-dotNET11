public sealed class PeriodicWork : IDisposable
{
    private readonly PeriodicTimer timer;
    private readonly TimeProvider clock;
    private readonly Func<CancellationToken, Task> work;
    public int Completed { get; private set; }
    public DateTimeOffset LastTick { get; private set; }
    public PeriodicWork(TimeProvider clock, Func<CancellationToken, Task> work)
    {
        this.clock = clock;
        this.work = work;
        timer = new PeriodicTimer(TimeSpan.FromSeconds(10), clock);
    }
    public async Task<bool> TickOnceAsync(CancellationToken token)
    {
        if (!await timer.WaitForNextTickAsync(token)) return false;
        LastTick = clock.GetUtcNow();
        await work(token);
        Completed++;
        return true;
    }
    public void Dispose() => timer.Dispose();
}
