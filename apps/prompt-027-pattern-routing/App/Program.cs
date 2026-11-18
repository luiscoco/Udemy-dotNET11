var cases = new (Order? Order, string Expected)[]
{
    (null, "Missing order"),
    (new(-1m, false, false), "Reject amount"),
    (new(2000m, false, true), "Await payment"),
    (new(1000m, true, true), "Customs review"),
    (new(999m, true, true), "Priority"),
    (new(100m, true, false), "Priority"),
    (new(99.99m, true, false), "Standard"),
    (new(0m, true, false), "Standard")
};
foreach (var item in cases)
{
    string decision = Routing.Decide(item.Order);
    Check.That(decision == item.Expected, $"expected route {item.Expected}");
    Check.That(decision == PreviousRouting.Decide(item.Order), "refactor preserves the previous decision");
}
Console.WriteLine($"Verified routing cases {cases.Length}");
