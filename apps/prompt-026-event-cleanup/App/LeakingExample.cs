public static class LeakingExample
{
    public static void Run()
    {
        var publisher = new InvoicePublisher();
        int calls = 0;
        publisher.Completed += (_, _) => calls++;
        publisher.Publish(154.68m);
        publisher.Completed -= (_, _) => calls++;
        publisher.Publish(154.68m);
        Console.WriteLine($"Handler calls after attempted cleanup {calls}");
        Check.That(calls == 1, "attempted anonymous-handler cleanup");
    }
}
