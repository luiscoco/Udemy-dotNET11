System.Globalization.CultureInfo.CurrentCulture =
    System.Globalization.CultureInfo.InvariantCulture;
if (args.FirstOrDefault() == "empty")
{
    LookupOutcome empty = default;
    Console.WriteLine(OutcomeView.Describe(empty));
    return;
}
Check.That(OrderLookup.Find(42) is Found { Net: 128.90m }, "success carries the order");
Check.That(OrderLookup.Find(99) is Missing { Number: 99 }, "missing data is explicit");
Check.That(OrderLookup.Find(-1) is Rejected, "invalid input carries a rejection");
foreach (int number in new[] { 42, 99, -1 })
    Console.WriteLine(OutcomeView.Describe(OrderLookup.Find(number)));
