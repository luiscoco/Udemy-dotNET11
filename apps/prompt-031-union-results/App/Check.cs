public static class Check
{
    public static void That(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"Check failed — {message}");
        Console.WriteLine($"PASS {message}");
    }
}
