using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
public sealed class TenantServices(DbContextOptions<TenantContext> options,string files,bool brokenCache):IDisposable
{
    private readonly MemoryCache cache=new(new MemoryCacheOptions());
    private readonly Dictionary<string,string> actors=new(){{"alice","tenant-a"},{"bob","tenant-b"}};
    public async Task<string> Read(string tenant,int id)
    {
        var key=brokenCache?$"row:{id}":$"v1:tenant:{Uri.EscapeDataString(tenant)}:row:{id}";
        return (await cache.GetOrCreateAsync(key,async entry=>
        {
            entry.AbsoluteExpirationRelativeToNow=TimeSpan.FromMinutes(1);
            await using var db=new TenantContext(options,new(tenant));
            return await db.Rows.Where(x=>x.Id==id).Select(x=>x.Title).SingleAsync();
        }))!;
    }
    public string FileFor(string tenant,string name)
    {
        if(name!=Path.GetFileName(name)||name.Contains(".."))throw new UnauthorizedAccessException("Invalid file identifier");
        var directory=Path.Combine(files,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tenant))));
        return Path.Combine(directory,name);
    }
    public Task<string> RunJob(TenantJob job)=>Task.Run(async()=>
    {
        if(!actors.TryGetValue(job.Actor,out var tenant)||tenant!=job.Tenant)throw new UnauthorizedAccessException("Job actor does not belong to tenant");
        // Explicit scope rehydration; no ambient HttpContext is available in the worker.
        await using var db=new TenantContext(options,new TenantScope(job.Tenant));
        return await db.Rows.Where(x=>x.Id==job.RowId).Select(x=>x.Title).SingleAsync();
    });
    public void Dispose()=>cache.Dispose();
}
public sealed record TenantJob(string Tenant,string Actor,int RowId);
