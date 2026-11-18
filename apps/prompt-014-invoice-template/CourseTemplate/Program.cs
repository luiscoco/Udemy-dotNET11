using TemplateApp;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

var invoice = new Invoice("Ada", 128.90m);
if (invoice.Total != 154.68m) throw new InvalidOperationException("Wrong invoice total");
Console.WriteLine($"{typeof(Invoice).Namespace}: customer={invoice.Customer}, total={invoice.Total:0.00}");
Console.WriteLine("PASS: generated invoice starter");
