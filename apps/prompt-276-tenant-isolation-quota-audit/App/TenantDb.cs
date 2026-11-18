using Microsoft.EntityFrameworkCore;

public sealed class TenantDb(DbContextOptions<TenantDb> options) : DbContext(options)
{
    public string Tenant { get; set; } = "";
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Document>().HasKey(x => x.Id);
        b.Entity<Document>().HasQueryFilter(x => x.Tenant == Tenant);
    }
}

public sealed class Document
{
    public string Id { get; set; } = "";
    public string Tenant { get; set; } = "";
    public string Content { get; set; } = "";
}

public sealed record NewDocument(string Id, string Content);
