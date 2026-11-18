using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
public static class CacheNodes
{
    public static ServiceProvider Create()
    {
        var services=new ServiceCollection();services.AddLogging();services.AddMemoryCache();
        services.AddStackExchangeRedisCache(o=>o.Configuration=
            Environment.GetEnvironmentVariable("COURSE_REDIS")??"127.0.0.1:16379");
        services.AddHybridCache(o=>o.DefaultEntryOptions=new HybridCacheEntryOptions
        {Expiration=TimeSpan.FromMinutes(1),LocalCacheExpiration=TimeSpan.FromSeconds(20)});
        return services.BuildServiceProvider();
    }
}
public sealed class Origin
{
    private int reads;
    public int Reads=>reads;
    public async Task<string> LoadAsync(string tenant,CancellationToken cancellationToken=default)
    {
        Interlocked.Increment(ref reads);await Task.Delay(50,cancellationToken);
        return $"{tenant}:course:seats=12";
    }
}
