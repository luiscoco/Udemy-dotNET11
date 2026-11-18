using System.Numerics;
public readonly record struct Amount(decimal Value) :
    IAdditionOperators<Amount, Amount, Amount>, IAdditiveIdentity<Amount, Amount>
{
    public static Amount AdditiveIdentity => new(0m);
    public static Amount operator +(Amount left, Amount right) =>
        new(checked(left.Value + right.Value));
}
