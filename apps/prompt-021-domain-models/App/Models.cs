public sealed class MutableOrder(string customer, decimal net)
{
    public string Customer { get; } = customer;
    public decimal Net { get; set; } = net;
}

public sealed record OrderSnapshot(string Customer, decimal Net);

public readonly record struct Money(decimal Amount, string Currency);

public sealed record TaggedOrder(string Customer, List<string> Tags);
