using System.Globalization;
using System.Text.Json;
using Course.Invoice.Contracts;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var invoice = new InvoiceSummary("Ada Lovelace", 128.90m, 25.78m);
Console.WriteLine($"Contracts assembly: {typeof(InvoiceSummary).Assembly.GetName().Version}");
Console.WriteLine(JsonSerializer.Serialize(invoice));
if (invoice.Total != 154.68m) throw new InvalidOperationException("Wrong receipt total");
Console.WriteLine("PASS: invoice total EUR 154.68");
