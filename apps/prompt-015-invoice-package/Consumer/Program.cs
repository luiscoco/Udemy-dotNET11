using Course.Invoice.Math;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

var total = InvoiceMath.Total(128.90m);
if (total != 154.68m) throw new InvalidOperationException("Unexpected total");
Console.WriteLine($"Package consumer total: {total:0.00}");
Console.WriteLine("PASS: packaged API produces the expected receipt");
