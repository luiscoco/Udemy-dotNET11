public static class TaxRules
{
    public static decimal Total(decimal net)
        => net + decimal.Round(net * 0.20m, 2, MidpointRounding.AwayFromZero);
}
