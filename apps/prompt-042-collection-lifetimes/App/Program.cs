using System.Collections.Immutable;
if (args.FirstOrDefault() == "ignored")
{
    var values = ImmutableDictionary<string, int>.Empty.Add("tax", 20);
    values.Add("shipping", 5);
    Console.WriteLine($"Original snapshot count {values.Count}");
    Check.That(values.Count == 2, "expected an ignored immutable update to mutate the snapshot");
    return;
}
Snapshots.Verify();
SharedCounts.Verify();
