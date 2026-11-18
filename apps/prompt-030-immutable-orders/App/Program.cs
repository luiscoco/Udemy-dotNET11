System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var source = new List<InvoiceLine> { new("Keyboard bundle", 1, 128.90m) };
if (args.FirstOrDefault() == "shared")
{
    var shared = new MutableInvoice(source);
    source.Add(new InvoiceLine("External edit", 1, 100m));
    Console.WriteLine($"Shared invoice total after external edit {shared.Total:0.00}");
    Check.That(shared.Total == 154.68m, "external list changed the stored invoice");
    return;
}

var original = new InvoiceSnapshot("Ada", source);
source.Add(new InvoiceLine("External edit", 1, 100m));
Check.That(original.Lines.Length == 1 && original.Total == 154.68m, "snapshot owns its collection contents");
var revised = original.Add(new InvoiceLine("Mouse", 1, 20m));
Check.That(original.Lines.Length == 1 && revised.Lines.Length == 2, "revision preserves the previous snapshot");
Check.That(revised.Total == 178.68m, "revised invoice has the expected total");
var copy = original with { };
Check.That(copy.Lines == original.Lines, "record copy can safely share the immutable collection");
Console.WriteLine($"Original {original.Total:0.00} / revised {revised.Total:0.00}");
