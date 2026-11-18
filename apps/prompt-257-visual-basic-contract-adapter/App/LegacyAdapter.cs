using Legacy.Contracts;
public static class LegacyAdapter
{
    public static decimal Total(decimal amount,decimal rate,bool reimplement)
        =>reimplement?Math.Round(amount*(1+rate),2):TaxCalculator.Total(amount,rate);
}
