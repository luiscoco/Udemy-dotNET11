Stock[] inventory = [new("A", 10), new("B", 20), new("D", 5)];
Stock[] warehouse = [new("a", 10), new("C", 30), new("D", 6)];
var pairs = Reconciliation.Pairs(inventory, warehouse);
if (args.FirstOrDefault() == "inneronly")
{
    var innerOnly = inventory.Join(warehouse, item => item.Sku, item => item.Sku,
        comparer: StringComparer.OrdinalIgnoreCase).ToArray();
    Console.WriteLine($"Inner join rows {innerOnly.Length}");
    Check.That(innerOnly.Length == 4, "expected unmatched rows in an inner join");
    return;
}
if (args.FirstOrDefault() == "unsafe")
{
    foreach (var pair in pairs) Console.WriteLine(pair.Outer!.Sku);
    return;
}
Check.That(pairs.Length == 4, "full join retains matched and unmatched keys");
Check.That(pairs.Count(pair => pair.Outer is null) == 1 && pairs.Count(pair => pair.Inner is null) == 1,
    "each missing side is represented explicitly");
Check.That(Reconciliation.Pairs(inventory, [.. warehouse, new("d", 7)]).Length == 5,
    "duplicate matching keys produce additional pairs");
foreach (var pair in pairs) Console.WriteLine(Reconciliation.Describe(pair));
