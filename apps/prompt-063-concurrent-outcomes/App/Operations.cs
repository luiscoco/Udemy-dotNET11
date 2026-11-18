public sealed record Outcome(string Name, int? Value, Exception? Error);
public static class Operations
{
    public static async Task<int> RunAsync(string name, Task release, Action started)
    {
        started();
        await release;
        await Task.Yield();
        return name switch
        {
            "inventory" => 42,
            "pricing" => throw new InvalidOperationException("Pricing feed unavailable"),
            _ => throw new IOException("Shipping feed disconnected")
        };
    }
    public static async Task<Outcome> ObserveAsync(string name, Task<int> operation)
    {
        try { return new(name, await operation, null); }
        catch (Exception error) { return new(name, null, error); }
    }
}
