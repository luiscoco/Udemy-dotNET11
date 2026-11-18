using Course.Functional;
public static class PricingAdapter
{
    public static Quote Calculate(int quantity,decimal price,bool unsafeDefault)
    {
        var result=Pricing.TryQuote(quantity,price);
        if(result.IsError)
        {
            if(unsafeDefault)return new Quote {Quantity=quantity,UnitPrice=price,Total=0m};
            throw new ArgumentException(result.ErrorValue);
        }
        return result.ResultValue;
    }
}
