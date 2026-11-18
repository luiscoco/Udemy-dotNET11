using Microsoft.Extensions.DependencyInjection;
var services = new ServiceCollection();
services.AddKeyedSingleton<IShippingRate, LocalRate>("local");
services.AddKeyedSingleton<IShippingRate, InternationalRate>("international");
services.AddTransient<QuoteRouter>();
services.AddTransient<LocalQuote>();
using var provider = services.BuildServiceProvider();
if (args.FirstOrDefault() == "unkeyed")
{
    _ = provider.GetRequiredService<IShippingRate>();
    return;
}
var router = provider.GetRequiredService<QuoteRouter>();
if (args.FirstOrDefault() == "unknown")
{
    _ = router.Calculate("premium", 2m);
    return;
}
Check.That(router.Calculate("local", 2m) == 14m && router.Calculate("international", 2m) == 30m,
    "one interface selects two registered rate implementations by explicit key");
Check.That(provider.GetRequiredService<LocalQuote>().Calculate(2m) == 14m,
    "FromKeyedServices binds the fixed local-rate consumer");
Check.That(ReferenceEquals(provider.GetRequiredKeyedService<IShippingRate>("local"),
    provider.GetRequiredKeyedService<IShippingRate>("local")), "the local key retains its registered singleton lifetime");
Console.WriteLine("Two kilograms: local 14.00; international 30.00");
