public sealed class Order
{
    private string customer = string.Empty;
    public int Number { get; }

    public required string Customer
    {
        get => customer;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            customer = value.Trim();
        }
    }

    public decimal Net { get; }
    public decimal Total => Net * 1.20m;

    public Order(int number, decimal net)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentOutOfRangeException.ThrowIfNegative(net);
        Number = number;
        Net = net;
    }
}
