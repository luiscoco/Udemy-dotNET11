using System.Collections;
public sealed record Order(string Customer, decimal Net);
public sealed class ObservedOrders(List<Order> orders) : IEnumerable<Order>
{
    public int Enumerations { get; private set; }
    public int Visits { get; private set; }
    public IEnumerator<Order> GetEnumerator()
    {
        Enumerations++;
        foreach (var order in orders) { Visits++; yield return order; }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
