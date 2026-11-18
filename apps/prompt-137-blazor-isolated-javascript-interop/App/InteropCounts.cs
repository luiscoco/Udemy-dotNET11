public sealed class InteropCounts
{
    private int disposed;
    public int Disposed => disposed;
    public void Removed() => Interlocked.Increment(ref disposed);
}
