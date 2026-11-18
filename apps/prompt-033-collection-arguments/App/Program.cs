string mode = args.FirstOrDefault() ?? "verify";
if (mode == "wrong")
{
    var wrong = Collections.CaseSensitiveCodes();
    Console.WriteLine($"Observed distinct codes {wrong.Count}");
    Check.That(wrong.Count == 1, "expected case-insensitive deduplication");
    return;
}
if (mode == "negative")
{
    List<string> invalid = [with(capacity: -1), "Ada"];
    Console.WriteLine(invalid.Count);
    return;
}
var names = Collections.Names(["Ada", "Grace", "Luis"]);
Check.That(names.Count == 3 && names.Capacity >= 6, "capacity reserves room while count tracks elements");
Check.That(names.SequenceEqual(new[] { "Ada", "Grace", "Luis" }), "spread preserves the values");
var codes = Collections.Codes();
Check.That(codes.Count == 1 && codes.Contains("pAiD"), "comparer determines equality");
Console.WriteLine($"Count {names.Count}, capacity {names.Capacity}, unique codes {codes.Count}");
