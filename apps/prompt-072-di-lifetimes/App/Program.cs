using Microsoft.Extensions.DependencyInjection;
var services = new ServiceCollection();
services.AddSingleton<SingletonStamp>();
services.AddScoped<ScopedStamp>();
services.AddTransient<TransientStamp>();
services.AddScoped<RequestService>();
if (args.FirstOrDefault() == "capture") services.AddSingleton<BadCache>();
using var provider = services.BuildServiceProvider(new ServiceProviderOptions
    { ValidateScopes = true, ValidateOnBuild = true });
if (args.FirstOrDefault() == "root")
{
    _ = provider.GetRequiredService<ScopedStamp>();
    return;
}
var singleton = provider.GetRequiredService<SingletonStamp>();
ScopedStamp first;
using (var scope = provider.CreateScope())
{
    var one = scope.ServiceProvider.GetRequiredService<RequestService>();
    var two = scope.ServiceProvider.GetRequiredService<RequestService>();
    first = one.Scoped;
    Check.That(ReferenceEquals(one, two) && ReferenceEquals(one.Scoped, two.Scoped), "scoped identities are reused within one scope");
    var transientOne = scope.ServiceProvider.GetRequiredService<TransientStamp>();
    var transientTwo = scope.ServiceProvider.GetRequiredService<TransientStamp>();
    Check.That(!ReferenceEquals(transientOne, transientTwo), "transient resolutions create distinct instances");
    Console.WriteLine($"Scope one {first.Id}; transient IDs {transientOne.Id}, {transientTwo.Id}");
}
Check.That(first.Disposed && !singleton.Disposed, "scope disposal releases its scoped instance while the singleton remains alive");
using (var scope = provider.CreateScope())
{
    var next = scope.ServiceProvider.GetRequiredService<RequestService>();
    Check.That(!ReferenceEquals(first, next.Scoped) && ReferenceEquals(singleton, next.Singleton),
        "a new scope gets new scoped state and the same container singleton");
    Console.WriteLine($"Scope two {next.Scoped.Id}; singleton {singleton.Id}");
}
provider.Dispose();
Check.That(singleton.Disposed, "the container owns singleton disposal");
