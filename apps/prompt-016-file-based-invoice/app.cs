#:property PublishAOT=false
#:property SelfContained=false
#:include Invoice.cs
#:include TaxRules.cs

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

var invoice = new Invoice("Ada", 128.90m);
Console.WriteLine($"Customer: {invoice.Customer}");
Console.WriteLine($"Total: {TaxRules.Total(invoice.Net):0.00}");
if (TaxRules.Total(invoice.Net) != 154.68m) throw new Exception("Wrong total");
Console.WriteLine("PASS: multi-file invoice application");
