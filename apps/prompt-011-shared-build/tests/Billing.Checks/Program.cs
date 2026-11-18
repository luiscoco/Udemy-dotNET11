using InvoiceSuite.Core;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    checks++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (ArgumentException) { Check(true, name); return; }
    throw new InvalidOperationException("FAIL: " + name);
}
var invoice = new Invoice("Ada", new InvoiceLine("Keyboard", 1, 79.90m),
    new InvoiceLine("Mouse", 2, 24.50m));
Check(invoice.Net == 128.90m && invoice.Tax == 25.78m && invoice.Total == 154.68m,
    "checkout totals match the teaching receipt");
Check(new Invoice("Rounding", new InvoiceLine("Small item", 1, 0.025m)).Tax == 0.01m,
    "half-cent tax rounds away from zero");
Reject(() => new Invoice(" ", new InvoiceLine("Item", 1, 1m)), "blank customer rejected");
Reject(() => new Invoice("Ada"), "empty invoice rejected");
Reject(() => new Invoice("Ada", new InvoiceLine("Item", 0, 1m)), "zero quantity rejected");
Reject(() => new Invoice("Ada", new InvoiceLine("Item", 1, -1m)), "negative price rejected");
Check(Invoice.CustomerNameLength(null) == 0 && Invoice.CustomerNameLength("Ada") == 3,
    "optional customer name handled explicitly");
var input = new[] { new InvoiceLine("Original", 1, 10m) };
var snapshot = new Invoice("Ada", input);
input[0] = new InvoiceLine("Changed", 1, 999m);
Check(snapshot.Net == 10m, "caller cannot replace the invoice line snapshot");
Console.WriteLine($"RESULT: {checks} checks passed");
