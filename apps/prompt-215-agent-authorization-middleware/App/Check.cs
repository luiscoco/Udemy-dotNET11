public static class Check
{
    public static void That(bool condition, string success, string? diagnostic = null)
    {
        if (!condition)
            throw new InvalidOperationException("Check failed — " + (diagnostic ?? success));
        Console.WriteLine("PASS " + success);
    }
}
