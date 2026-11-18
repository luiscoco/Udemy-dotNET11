var input = new List<Order> { new("Ada", 120m), new("Grace", 40m), new("Luis", 150m) };
var observed = new ObservedOrders(input);
var query = observed.Where(order => order.Net >= 100m).Select(order => order.Customer);
Check.That(observed.Enumerations == 0, "building the query has not visited the source");
if (args.FirstOrDefault() == "repeated")
{
    bool any = query.Any();
    string[] again = query.ToArray();
    Console.WriteLine($"Enumerations {observed.Enumerations}, visits {observed.Visits}");
    Check.That(observed.Enumerations == 1, "expected Any and ToArray to share one enumeration");
    return;
}
string[] snapshot = query.ToArray();
Check.That(snapshot.SequenceEqual(new[] { "Ada", "Luis" }), "materialization records the selected names");
Check.That(observed.Enumerations == 1 && observed.Visits == 3, "one materialization visits three source rows");
input.Add(new("New customer", 200m));
Check.That(snapshot.Length == 2 && query.Count() == 3, "snapshot and deferred query observe different source times");
Console.WriteLine($"Snapshot names {snapshot.Length}, enumerations {observed.Enumerations}, visits {observed.Visits}");
