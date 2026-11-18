using System.Globalization;
using System.Reflection;
using InvoiceSuite.Core;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var region = Assembly.GetEntryAssembly()!
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .Single(attribute => attribute.Key == "TrainingRegion").Value;
var invoice = new Invoice("Ada Lovelace",
    new InvoiceLine("Keyboard", 1, 79.90m),
    new InvoiceLine("Mouse", 2, 24.50m));

Console.WriteLine($"CHECKOUT | training region: {region}");
Console.WriteLine($"Customer: {invoice.Customer}");
foreach (var line in invoice.Lines)
    Console.WriteLine($"{line.Description,-10} x{line.Quantity}  EUR {line.Net:F2}");
Console.WriteLine($"Net:          EUR {invoice.Net:F2}");
Console.WriteLine($"Tax (20%):    EUR {invoice.Tax:F2}");
Console.WriteLine($"TOTAL:        EUR {invoice.Total:F2}");
Console.WriteLine("Region is a build-metadata label; tax is a fixed teaching rule.");
