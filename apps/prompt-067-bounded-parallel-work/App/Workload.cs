public sealed class Workload
{
    private int active, peak, bytes, peakBytes;
    public int Peak => peak;
    public int PeakBytes => peakBytes;
    private static void Maximum(ref int field, int value)
    {
        int seen;
        do { seen = field; if (value <= seen) return; }
        while (Interlocked.CompareExchange(ref field, value, seen) != seen);
    }
    public async Task<int> ProcessAsync(int value, CancellationToken token, Task? gate = null)
    {
        byte[] buffer = new byte[4096];
        Maximum(ref peak, Interlocked.Increment(ref active));
        Maximum(ref peakBytes, Interlocked.Add(ref bytes, buffer.Length));
        try
        {
            if (gate is not null) await gate;
            await Task.Delay(10, token); // Controlled I/O-like wait, not external network traffic.
            buffer[0] = (byte)value;
            return buffer[0] * 2;
        }
        finally
        {
            Interlocked.Add(ref bytes, -buffer.Length);
            Interlocked.Decrement(ref active);
        }
    }
}
