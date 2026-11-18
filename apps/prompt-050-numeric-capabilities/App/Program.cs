using System.Globalization;
using System.Numerics;
if (args.FirstOrDefault() == "precision")
{
    var large = Decimal32.Parse("10000000", CultureInfo.InvariantCulture);
    var incremented = large + Decimal32.One;
    Console.WriteLine($"Decimal32 value after unit increment {incremented}");
    Check.That(incremented > large, "expected a unit increment beyond the available precision");
    return;
}
if (args.FirstOrDefault() == "divide")
{
    decimal zero = 0m;
    Console.WriteLine(1m / zero);
    return;
}
NumericSamples.VerifyDecimalFloatingPoint();
NumericSamples.VerifyComplex();
