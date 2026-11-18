using System.Text.Json;
var previous = new PreviousInvoice(42, 128.90m) { Customer = "  Ada \"A\"  " };
var invoice = new Invoice(42, 128.90m) { Customer = previous.Customer };
if (args.FirstOrDefault() == "blank") { invoice.Customer = " "; return; }
Check.That(invoice.Number == previous.Number && invoice.Net == previous.Net,
    "primary constructor preserves invoice identity and amount");
Check.That(invoice.Customer == "Ada \"A\"" && invoice.Total == previous.Total,
    "validated property and total preserve the earlier behavior");
using var before = JsonDocument.Parse(previous.ToJson());
using var after = JsonDocument.Parse(invoice.ToJson());
foreach (string property in new[] { "number", "customer", "total" })
    Check.That(before.RootElement.GetProperty(property).ToString() ==
        after.RootElement.GetProperty(property).ToString(), $"JSON field {property} is preserved");
Console.WriteLine(invoice.ToJson());
