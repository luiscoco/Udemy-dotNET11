public sealed class Checkout(ITaxPolicy taxPolicy)
{
    public decimal Total(decimal net)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(net);
        decimal tax = taxPolicy.Calculate(net);
        if (tax < 0)
            throw new InvalidOperationException("Tax policy returned a negative amount");
        return net + tax;
    }
}
