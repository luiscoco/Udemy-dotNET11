using System.Net;
using Microsoft.Extensions.DependencyInjection;
await using var server = new CatalogServer();
await server.StartAsync();
var services = new ServiceCollection();
services.AddHttpClient<CatalogClient>(http =>
{
    http.BaseAddress = new Uri(server.Address + "/api/");
    http.Timeout = TimeSpan.FromSeconds(2);
    http.DefaultRequestVersion = HttpVersion.Version11;
    http.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
    MaxConnectionsPerServer = 2,
    UseCookies = false
}).SetHandlerLifetime(Timeout.InfiniteTimeSpan);
using var provider = services.BuildServiceProvider();
var first = provider.GetRequiredService<CatalogClient>();
var second = provider.GetRequiredService<CatalogClient>();
if (args.FirstOrDefault() == "status") { _ = await first.ReadAsync(0); return; }
if (args.FirstOrDefault() == "timeout") { _ = await first.SlowAsync(); return; }
CatalogItem[] values = await Task.WhenAll(Enumerable.Range(1, 6)
    .Select(i => (i % 2 == 0 ? first : second).ReadAsync(i)));
Check.That(values.Select(x => x.Id).SequenceEqual(Enumerable.Range(1, 6)), "the typed clients deserialize every real loopback response");
Check.That(!ReferenceEquals(first, second), "typed client resolutions are distinct transient consumers");
Check.That(server.Peak <= 2, "the two consumers share the configured HTTP/1.1 connection pool limit");
Console.WriteLine($"Items {values.Length}; observed peak HTTP/1.1 requests {server.Peak}");
Console.WriteLine("Handler lifetime is infinite here; pooled connections have their own finite rotation and idle limits.");
