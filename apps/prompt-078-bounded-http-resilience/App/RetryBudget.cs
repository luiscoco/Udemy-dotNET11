public sealed class RetryBudget(int initial)
{
    private int remaining = initial;
    public int Used => initial - Volatile.Read(ref remaining);
    public bool TrySpend()
    {
        int seen;
        do { seen = Volatile.Read(ref remaining); if (seen == 0) return false; }
        while (Interlocked.CompareExchange(ref remaining, seen - 1, seen) != seen);
        return true;
    }
}
