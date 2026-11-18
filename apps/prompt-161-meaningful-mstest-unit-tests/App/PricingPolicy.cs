public sealed class PricingPolicy
{
    public decimal Total(int quantity, decimal unitPrice)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        decimal multiplier = quantity >= 3 ? 0.9m : 1m;
        return decimal.Round(quantity * unitPrice * multiplier, 2, MidpointRounding.AwayFromZero);
    }
}
