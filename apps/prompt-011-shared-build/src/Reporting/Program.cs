using System.Globalization;
using System.Reflection;
using InvoiceSuite.Core;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var region = Assembly.GetEntryAssembly()!
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .Single(attribute => attribute.Key == "TrainingRegion").Value;
var invoices = new[]
{
    new Invoice("Ada Lovelace", new InvoiceLine("Keyboard", 1, 79.90m),
        new InvoiceLine("Mouse", 2, 24.50m)),
    new Invoice("Grace Hopper", new InvoiceLine("Monitor", 1, 250.00m))
};

Console.WriteLine($"REPORTING | training region: {region}");
Console.WriteLine($"Invoices: {invoices.Length}");
Console.WriteLine($"Net sales:    EUR {invoices.Sum(invoice => invoice.Net):F2}");
Console.WriteLine($"Tax:          EUR {invoices.Sum(invoice => invoice.Tax):F2}");
Console.WriteLine($"Gross sales:  EUR {invoices.Sum(invoice => invoice.Total):F2}");
Console.WriteLine("Region is a build-metadata label; tax is a fixed teaching rule.");
