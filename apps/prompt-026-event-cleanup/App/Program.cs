if (args.FirstOrDefault() == "leak")
{
    LeakingExample.Run();
    return;
}

TaxCalculation tax = net => decimal.Round(net * 0.20m, 2);
Check.That(tax(128.90m) == 25.78m, "delegate invokes the supplied lambda");
var publisher = new InvoicePublisher();
int calls = 0;
decimal observedTotal = 0m;
var subscription = new ReceiptSubscription(publisher, (_, invoice) =>
{
    calls++;
    observedTotal = invoice.Total;
});
using (subscription)
{
    publisher.Publish(154.68m);
}
subscription.Dispose();
publisher.Publish(200m);
Check.That(calls == 1 && observedTotal == 154.68m, "disposed subscription receives no later invoice");
Console.WriteLine($"Final subscriber calls {calls}");
