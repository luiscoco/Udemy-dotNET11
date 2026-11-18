System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var standard = new Checkout(new StandardTax());
var zero = new Checkout(new ZeroTax());
Check.That(standard.Total(128.90m) == 154.68m, "composed standard policy produces the expected receipt");
Check.That(zero.Total(128.90m) == 128.90m, "interchangeable zero policy changes the business calculation");
Console.WriteLine($"Standard {standard.Total(128.90m):0.00} / zero {zero.Total(128.90m):0.00}");
try
{
    new Checkout(new InvalidTax()).Total(10m);
    throw new Exception("Invalid policy was accepted");
}
catch (InvalidOperationException error) when (error.Message.Contains("negative"))
{
    Console.WriteLine("PASS invalid policy behavior rejected");
}

public sealed class InvalidTax : ITaxPolicy
{
    public decimal Calculate(decimal net) => -1m;
}
