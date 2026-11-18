public sealed class Account
{
    private readonly object sync = new();
    private int available = 1000, reserved;
    public void Reserve()
    {
        lock (sync)
        {
            if (available == 0) return;
            available--;
            reserved++;
        }
    }
    public (int Available, int Reserved) Snapshot()
    {
        lock (sync) return (available, reserved);
    }
}
