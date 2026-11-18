using System.Runtime.CompilerServices;

public static class ImportService
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Load()
    {
        try
        {
            InvoiceStore.Load();
        }
        catch (IOException error)
        {
            Console.WriteLine($"Recorded storage failure {error.Message}");
            throw;
        }
    }
}
