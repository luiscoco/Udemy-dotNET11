using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
public sealed class SearchContext(DbContextOptions<SearchContext> options):DbContext(options)
{
    public DbSet<Article> Articles=>Set<Article>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Article>().ToTable("Articles");
        model.HasFullTextCatalog("courseFT");
        model.Entity<Article>().HasFullTextIndex(x=>x.Contents)
            .UseKeyIndex("PK_Articles").UseCatalog("courseFT");
    }
}
public sealed class Article
{
    public int Id {get;set;}
    public string Title {get;set;}="";
    public string Contents {get;set;}="";
}
public sealed class SearchDesignFactory:IDesignTimeDbContextFactory<DeploymentContext>
{
    public DeploymentContext CreateDbContext(string[] args)=>new(
        new DbContextOptionsBuilder<DeploymentContext>().UseSqlServer(
            "Server=127.0.0.1,15433;Database=design;User Id=sa;Password=Course_11_Local!;TrustServerCertificate=True").Options);
}


// Isolate the installed provider's unstable full-text annotations from schema validation.
// Full-text DDL remains in the reviewed migration; SearchContext retains its native mapping.
public sealed class DeploymentContext(DbContextOptions<DeploymentContext> options):DbContext(options)
{
    public DbSet<Article> Articles=>Set<Article>();
    protected override void OnModelCreating(ModelBuilder model)=>model.Entity<Article>().ToTable("Articles");
}
