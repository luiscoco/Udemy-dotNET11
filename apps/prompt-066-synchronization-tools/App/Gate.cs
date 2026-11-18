public sealed class Gate
{
    private readonly SemaphoreSlim slots = new(2, 2);
    private int active, peak;
    public int Peak => peak;
    public async Task UseAsync()
    {
        await slots.WaitAsync();
        try
        {
            int now = Interlocked.Increment(ref active);
            int observed;
            do { observed = peak; if (now <= observed) break; }
            while (Interlocked.CompareExchange(ref peak, now, observed) != observed);
            await Task.Delay(1);
        }
        finally { Interlocked.Decrement(ref active); slots.Release(); }
    }
}
