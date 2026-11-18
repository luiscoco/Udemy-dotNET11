using System.Runtime.CompilerServices;
public static class CpuWork
{
    public static long Computations;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static long Compute(int key)
    {
        Interlocked.Increment(ref Computations);
        long total = key;
        for(int i=0;i<500000;i++) total = unchecked(total * 31 + i);
        return total;
    }
}
