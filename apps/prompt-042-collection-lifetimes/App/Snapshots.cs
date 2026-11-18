using System.Collections.Immutable;
using System.Collections.Frozen;
public static class Snapshots
{
    public static void Verify()
    {
        var original = ImmutableDictionary<string, int>.Empty.Add("tax", 20);
        var revised = original.Add("shipping", 5);
        Check.That(original.Count == 1 && revised.Count == 2, "immutable update returns a new snapshot");
        var source = new Dictionary<string, int> { ["tax"] = 20 };
        var frozen = source.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        source["tax"] = 99;
        Check.That(frozen["TAX"] == 20, "frozen lookup keeps the construction-time data");
    }
}
