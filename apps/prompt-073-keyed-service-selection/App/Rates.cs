using Microsoft.Extensions.DependencyInjection;
public interface IShippingRate { decimal Quote(decimal kilograms); }
public sealed class LocalRate : IShippingRate
{
    public decimal Quote(decimal kilograms) => 10m + 2m * kilograms;
}
public sealed class InternationalRate : IShippingRate
{
    public decimal Quote(decimal kilograms) => 20m + 5m * kilograms;
}
public sealed class LocalQuote([FromKeyedServices("local")] IShippingRate rate)
{
    public decimal Calculate(decimal kilograms) => rate.Quote(kilograms);
}
public sealed class QuoteRouter(IServiceProvider provider)
{
    public decimal Calculate(string choice, decimal kilograms)
    {
        string key = choice switch
        {
            "local" => "local", "international" => "international",
            _ => throw new ArgumentException("Unknown shipping choice")
        };
        return provider.GetRequiredKeyedService<IShippingRate>(key).Quote(kilograms);
    }
}
