public interface ITaxPolicy
{
    decimal Calculate(decimal net);
}

public abstract class TaxPolicy : ITaxPolicy
{
    public abstract decimal Calculate(decimal net);
    protected static decimal Round(decimal tax) => decimal.Round(tax, 2);
}

public sealed class StandardTax : TaxPolicy
{
    public override decimal Calculate(decimal net) => Round(net * 0.20m);
}

public sealed class ZeroTax : ITaxPolicy
{
    public decimal Calculate(decimal net) => 0m;
}
