using System.Globalization;
using System.Numerics;
public static class NumericSamples
{
    public static Decimal64 Read(string value) => Decimal64.Parse(value, CultureInfo.InvariantCulture);
    public static void VerifyDecimalFloatingPoint()
    {
        Check.That(0.1 + 0.2 != 0.3, "binary double demonstrates representation rounding");
        Check.That(0.1m + 0.2m == 0.3m, "decimal literals retain this base-ten sum");
        Check.That(Read("0.1") + Read("0.2") == Read("0.3"), "Decimal64 retains this base-ten sum");
        Check.That(Decimal64.IsPositiveInfinity(Decimal64.One / Decimal64.Zero), "IEEE decimal division yields infinity");
        Check.That(Decimal64.IsNaN(Decimal64.Zero / Decimal64.Zero), "IEEE decimal zero over zero yields NaN");
        Check.That(Decimal128.Parse("10000000", CultureInfo.InvariantCulture) + Decimal128.One >
            Decimal128.Parse("10000000", CultureInfo.InvariantCulture), "wider decimal floating point retains this increment");
    }

    public static void VerifyComplex()
    {
        var binary = new Complex(3, 4);
        var single = new Complex<float>(3f, 4f);
        Check.That(binary.Magnitude == 5 && Complex<float>.Abs(single) == 5f, "existing and generic complex magnitudes agree");
        var first = new Complex<Decimal64>(Read("1"), Read("2"));
        var second = new Complex<Decimal64>(Read("3"), Read("4"));
        var product = first * second;
        Check.That(product.Real == Read("-5") && product.Imaginary == Read("10"),
            "generic decimal complex multiplication yields minus five plus ten i");
        Console.WriteLine($"Decimal complex product {product}");
    }
}
