System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var order = new Order(42, 128.90m) { Customer = " Ada " };
Check.That(order.Customer == "Ada" && order.Total == 154.68m, "valid construction establishes expected order values");
Console.WriteLine($"Order {order.Number} / {order.Customer} / total {order.Total:0.00}");
Reject(() => new Order(0, 10m) { Customer = "Ada" }, "zero order number");
Reject(() => new Order(1, -1m) { Customer = "Ada" }, "negative net");
Reject(() => new Order(1, 10m) { Customer = " " }, "blank required customer");

static void Reject(Action create, string name)
{
    try { create(); }
    catch (ArgumentException)
    {
        Console.WriteLine($"PASS rejected {name}");
        return;
    }
    throw new InvalidOperationException($"Accepted {name}");
}
