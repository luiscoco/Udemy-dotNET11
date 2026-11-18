if (args.FirstOrDefault() == "overflow")
{
    Console.WriteLine(Numeric.SumProducts<int>([int.MaxValue], [2]));
    return;
}
Check.That(Numeric.SumProducts<int>([2, 3], [10, 5]) == 35, "integer products total 35");
Check.That(Numeric.SumProducts<decimal>([3m, 2m], [19.99m, 47.355m]) == 154.68m,
    "decimal calculation retains the invoice amount");
Check.That(Math.Abs(Numeric.SumProducts<double>([0.5], [3.0]) - 1.5) < 0.000001,
    "double calculation uses the same constrained algorithm");
Check.That(Numeric.SumProducts<int>([], []) == 0, "empty input uses the additive identity");
Check.That(Numeric.Sum<Amount>([new(20m), new(5m)]).Value == 25m,
    "custom amount implements the narrow static addition contract");
Check.That(Numeric.Sum<double>([1.0, 2.0, 3.0]) == 6.0,
    "built-in type satisfies the same narrow contract");
Check.That(Numeric.SumProducts<decimal>([int.MaxValue], [2m]) == 4294967294m,
    "decimal can represent this larger total");
try { Numeric.SumProducts<int>([1], []); }
catch (ArgumentException) { Console.WriteLine("PASS mismatched inputs are rejected"); return; }
throw new InvalidOperationException("Mismatched lengths were accepted");
