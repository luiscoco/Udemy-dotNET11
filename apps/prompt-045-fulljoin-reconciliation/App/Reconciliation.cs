public sealed record Stock(string Sku, int Quantity);
public static class Reconciliation
{
    public static (Stock? Outer, Stock? Inner)[] Pairs(Stock[] inventory, Stock[] warehouse) =>
        inventory.FullJoin(warehouse, item => item.Sku, item => item.Sku,
            comparer: StringComparer.OrdinalIgnoreCase).ToArray();

    public static string Describe((Stock? Outer, Stock? Inner) pair) => pair switch
    {
        (null, Stock right) => $"Warehouse only {right.Sku}",
        (Stock left, null) => $"Inventory only {left.Sku}",
        (Stock left, Stock right) => $"{left.Sku} difference {right.Quantity - left.Quantity}",
        _ => throw new InvalidOperationException("A join row needs at least one side")
    };
}
