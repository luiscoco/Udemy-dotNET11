using System.Collections.Concurrent;
public static class SharedCounts
{
    public static void Verify()
    {
        var counts = new ConcurrentDictionary<string, int>();
        Parallel.For(0, 200, _ => counts.AddOrUpdate("paid", 1, (_, current) => current + 1));
        Check.That(counts["paid"] == 200, "concurrent updates retain all 200 increments");
        Console.WriteLine($"Shared count {counts["paid"]}");
    }
}
