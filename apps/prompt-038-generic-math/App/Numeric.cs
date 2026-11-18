using System.Numerics;
public static class Numeric
{
    public static T SumProducts<T>(ReadOnlySpan<T> quantities, ReadOnlySpan<T> prices)
        where T : INumber<T>
    {
        if (quantities.Length != prices.Length)
            throw new ArgumentException("Quantities and prices must have equal lengths");
        T total = T.Zero;
        for (int index = 0; index < quantities.Length; index++)
            total = checked(total + checked(quantities[index] * prices[index]));
        return total;
    }

    public static T Sum<T>(ReadOnlySpan<T> values)
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        T total = T.AdditiveIdentity;
        foreach (T value in values) total += value;
        return total;
    }
}
