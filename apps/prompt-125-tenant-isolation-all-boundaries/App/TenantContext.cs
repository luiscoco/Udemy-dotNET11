using Microsoft.EntityFrameworkCore;
public sealed record TenantScope(string Id);
public sealed class TenantRow
{public string TenantId {get;set;}="";public int Id {get;set;}public string Title {get;set;}="";}
public sealed class TenantContext(DbContextOptions<TenantContext> options,TenantScope tenant):DbContext(options)
{
    public string TenantId=>tenant.Id;
    public DbSet<TenantRow> Rows=>Set<TenantRow>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<TenantRow>().HasKey(x=>new{x.TenantId,x.Id});
        model.Entity<TenantRow>().HasQueryFilter(x=>x.TenantId==TenantId);
    }
}
