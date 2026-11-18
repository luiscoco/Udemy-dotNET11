using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
public static class ReadStrategies
{
    public static Task<string?> Memory(IMemoryCache cache,string key,Origin origin)
        =>cache.GetOrCreateAsync(key,async entry=>
        {entry.AbsoluteExpirationRelativeToNow=TimeSpan.FromMinutes(1);return await origin.LoadAsync("tenant-a");});
    public static async Task<string> Distributed(IDistributedCache cache,string key,Origin origin)
    {
        var cached=await cache.GetStringAsync(key);
        if(cached is not null)return cached;
        var value=await origin.LoadAsync("tenant-a");
        await cache.SetStringAsync(key,value,new DistributedCacheEntryOptions{AbsoluteExpirationRelativeToNow=TimeSpan.FromMinutes(1)});
        return value;
    }
}
