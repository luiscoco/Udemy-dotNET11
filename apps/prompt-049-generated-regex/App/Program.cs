using System.Text.RegularExpressions;
if (args.FirstOrDefault() == "unicode")
{
    bool accepted = InvoicePatterns.UnicodeDigits().IsMatch("AB-٠٠٤٢");
    Console.WriteLine($"Unicode digit pattern accepted non-ASCII digits {accepted}");
    Check.That(!accepted, "invoice identifiers require ASCII digits");
    return;
}
if (args.FirstOrDefault() == "timeout")
{
    Console.WriteLine(InvoicePatterns.Risky().IsMatch(new string('a', 6000) + "!"));
    return;
}
var pattern = InvoicePatterns.Code();
Check.That(pattern.IsMatch("AB-0042"), "representative invoice code matches");
foreach (string value in new[] { "ab-0042", "AB-0042\n", "AB-42", " AB-0042", "AB-٠٠٤٢", "" })
    Check.That(!pattern.IsMatch(value), "edge case is rejected");
Check.That(pattern.MatchTimeout == TimeSpan.FromMilliseconds(100), "generated matcher has the configured timeout");
Console.WriteLine($"Generated matcher {pattern.GetType().Name}, timeout {pattern.MatchTimeout.TotalMilliseconds} ms");
