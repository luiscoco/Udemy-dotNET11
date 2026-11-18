public sealed record Order(int Id, string CustomerId, int Quantity, decimal Price)
{
    public decimal Net => Quantity * Price;
}
public sealed record Customer(string Id, string Name);
public static class Report
{
    public static IEnumerable<Order> Eligible(IEnumerable<Order> orders) =>
        orders.Where(order => order.Net >= 50m);

    public static (string CustomerId, string Customer, decimal Total)[] Build(IEnumerable<Order> orders, Customer[] customers) =>
        (from order in orders
         join customer in customers on order.CustomerId equals customer.Id
         select new { customer.Id, customer.Name, order.Net })
        .GroupBy(row => (row.Id, row.Name))
        .Select(group => (CustomerId: group.Key.Id, Customer: group.Key.Name, Total: group.Sum(row => row.Net)))
        .OrderBy(row => row.Customer).ThenBy(row => row.CustomerId)
        .ToArray();
}
