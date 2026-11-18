using System.Globalization;
using System.Text;
var spanish = CultureInfo.GetCultureInfo("es-ES");
var english = CultureInfo.GetCultureInfo("en-US");
if (args.FirstOrDefault() == "ambiguous")
{
    decimal first = decimal.Parse("1,234", NumberStyles.Number, english);
    decimal second = decimal.Parse("1,234", NumberStyles.Number, spanish);
    Console.WriteLine($"English value {first.ToString(CultureInfo.InvariantCulture)}, Spanish value {second.ToString(CultureInfo.InvariantCulture)}");
    Check.That(first == second, "expected the same interpretation from different cultures");
    return;
}
Check.That(1234.56m.ToString("N2", spanish) == "1.234,56", "Spanish display uses its explicit separators");
Check.That(1234.56m.ToString("N2", english) == "1,234.56", "English display uses its explicit separators");
Check.That(TextRules.ReadProtocolAmount("1234.56") == 1234.56m, "protocol parsing uses an explicit invariant rule");
Check.That(!decimal.TryParse("1,234.56", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _),
    "protocol rule rejects a grouping separator");
string composed = "Café", decomposed = "Café";
Check.That(!string.Equals(composed, decomposed, StringComparison.Ordinal) && TextRules.SameName(composed, decomposed),
    "normalization aligns canonically equivalent names");
string emoji = "A😀";
Check.That(emoji.Length == 3 && emoji.EnumerateRunes().Count() == 2, "UTF-16 units and Unicode scalar counts differ");
Check.That(StringInfo.ParseCombiningCharacters(decomposed).Length == 4, "text elements retain the combined accent");
Check.That(TextRules.SameCode("file", "FILE"), "identifier equality uses an explicit ordinal rule");
Console.WriteLine($"Spanish {1234.56m.ToString("N2", spanish)}, English {1234.56m.ToString("N2", english)}");
