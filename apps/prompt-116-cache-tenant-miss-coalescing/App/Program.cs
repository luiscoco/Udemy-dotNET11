using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
await using var nodeA=CacheNodes.Create();await using var nodeB=CacheNodes.Create();
var cache=nodeA.GetRequiredService<HybridCache>();var other=nodeB.GetRequiredService<HybridCache>();
var run="course116:"+Guid.NewGuid().ToString("N");var origin=new Origin();
string Key(string tenant)=>args.FirstOrDefault()=="tenant"?run+":course:1":CacheKeys.Course(run,tenant,1);
var a=await cache.GetOrCreateAsync(Key("tenant-a"),ct=>new ValueTask<string>(origin.LoadAsync("tenant-a",ct)));
var b=await cache.GetOrCreateAsync(Key("tenant-b"),ct=>new ValueTask<string>(origin.LoadAsync("tenant-b",ct)));
if(args.FirstOrDefault()=="tenant")Check.That(b.StartsWith("tenant-b:"),"incorrect expectation: a tenant-free key isolates tenants");
Check.That(a.StartsWith("tenant-a:") && b.StartsWith("tenant-b:") && origin.Reads==2,"versioned tenant keys isolate cached representations");
await cache.RemoveAsync(Key("tenant-a"));
_=await cache.GetOrCreateAsync(Key("tenant-a"),ct=>new ValueTask<string>(origin.LoadAsync("tenant-a",ct)));
Check.That(origin.Reads==3,"explicit invalidation forces a fresh origin read");
var burstOrigin=new Origin();var burstKey=CacheKeys.Course(run,"burst",1);
var burst=Enumerable.Range(0,20).Select(_=>cache.GetOrCreateAsync(burstKey,
    ct=>new ValueTask<string>(burstOrigin.LoadAsync("burst",ct))).AsTask());
await Task.WhenAll(burst);
Check.That(burstOrigin.Reads==1,"twenty concurrent misses share one factory within this cache instance");
var expiryOrigin=new Origin();var expiryKey=CacheKeys.Course(run,"expiry",1);
var shortEntry=new HybridCacheEntryOptions{Expiration=TimeSpan.FromMilliseconds(150),LocalCacheExpiration=TimeSpan.FromMilliseconds(150)};
_=await cache.GetOrCreateAsync(expiryKey,ct=>new ValueTask<string>(expiryOrigin.LoadAsync("expiry",ct)),shortEntry);
await Task.Delay(350);
_=await cache.GetOrCreateAsync(expiryKey,ct=>new ValueTask<string>(expiryOrigin.LoadAsync("expiry",ct)),shortEntry);
Check.That(expiryOrigin.Reads==2,"expired local and distributed entries reload the origin");
var gate=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);int entered=0;
async ValueTask<string> BothFactories(CancellationToken cancellationToken)
{if(Interlocked.Increment(ref entered)==2)gate.TrySetResult();await gate.Task.WaitAsync(TimeSpan.FromSeconds(10),cancellationToken);return "shared";}
var crossKey=CacheKeys.Course(run,"cross-node",1);
await Task.WhenAll(cache.GetOrCreateAsync(crossKey,BothFactories).AsTask(),other.GetOrCreateAsync(crossKey,BothFactories).AsTask());
Check.That(entered==2,"different cache instances do not provide a global distributed single-flight lock");
foreach(var key in new[]{Key("tenant-a"),Key("tenant-b"),burstKey,expiryKey,crossKey})await cache.RemoveAsync(key);
Console.WriteLine("service: isolation, invalidation, expiration and local/cross-node miss boundaries verified");
