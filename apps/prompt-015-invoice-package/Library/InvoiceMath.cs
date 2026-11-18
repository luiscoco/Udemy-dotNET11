namespace Course.Invoice.Math;

/// <summary>Calculates totals for a fixed-rate course example.</summary>
public static class InvoiceMath
{
    /// <summary>Returns net plus 20 percent teaching tax.</summary>
    /// <param name="net">A non-negative net amount.</param>
    /// <returns>The total with tax rounded to two decimal places.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Net is negative.</exception>
    public static decimal Total(decimal net)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(net);
        return net + decimal.Round(net * 0.20m, 2, MidpointRounding.AwayFromZero);
    }
}
