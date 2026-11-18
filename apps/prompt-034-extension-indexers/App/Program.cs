using CatalogIndexing;
Invoice[] invoices = [new("A", 10m), new("B", 20m), new("C", 30m)];
var catalog = new InvoiceCatalog(invoices);
Check.That(catalog["C"]?.Net == 30m, "extension indexer finds the requested invoice");
Check.That(catalog.Inspections == 3, "lookup exposes a three-element scan");
Console.WriteLine($"Last invoice required {catalog.Inspections} inspections");
Check.That(catalog["missing"] is null && catalog.Inspections == 3, "missing lookup scans the catalog");
var indexed = invoices.ToDictionary(invoice => invoice.Code);
Check.That(indexed["C"].Net == 30m, "dictionary provides an indexed alternative");
