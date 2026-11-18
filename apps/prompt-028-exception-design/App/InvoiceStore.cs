using System.Runtime.CompilerServices;

public static class InvoiceStore
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Load()
    {
        throw new IOException("Exercise store unavailable");
    }
}
