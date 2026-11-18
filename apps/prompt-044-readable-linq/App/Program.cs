Customer[] customers = [new("A", "Ada"), new("B", "Grace")];
Order[] orders = [new(1, "A", 2, 40m), new(2, "A", 1, 60m),
    new(3, "B", 1, 50m), new(4, "missing", 1, 150m)];
IEnumerable<Order> eligible = args.FirstOrDefault() == "boundary"
    ? orders.Where(order => order.Net > 50m)
    : Report.Eligible(orders);
var summaries = Report.Build(eligible, customers);
Console.WriteLine(string.Join(", ", summaries.Select(row => $"{row.Customer}={row.Total}")));
Check.That(summaries.Length == 2, "both known customers retain their boundary-qualified orders");
Check.That(summaries.Single(row => row.Customer == "Ada").Total == 140m, "grouped Ada total is 140");
Check.That(summaries.Single(row => row.Customer == "Grace").Total == 50m, "inclusive boundary retains Grace");
Check.That(summaries.Sum(row => row.Total) == 190m, "known-customer aggregation is 190");
Check.That(orders.Sum(order => order.Net) == 340m, "all-order total includes the unmatched customer row");

var sameNames = Report.Build([new(5, "X", 1, 60m), new(6, "Y", 1, 70m)],
    [new("X", "Same name"), new("Y", "Same name")]);
Check.That(sameNames.Length == 2 && sameNames.Single(row => row.CustomerId == "X").Total == 60m,
    "customer identity keeps equal display names in separate groups");
