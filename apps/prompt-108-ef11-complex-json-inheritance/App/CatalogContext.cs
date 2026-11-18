using Microsoft.EntityFrameworkCore;
public sealed class CatalogContext : DbContext
{
    public DbSet<Product> Products=>Set<Product>();
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=:memory:");
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().UseTptMappingStrategy();
        model.Entity<Product>().ComplexProperty(x=>x.Details,b=>b.ToJson());
        model.Entity<VideoCourse>().ToTable("VideoCourses");
    }
}
public abstract class Product
{
    public int Id {get;set;}
    public string Name {get;set;}="";
    public ProductDetails Details {get;set;}=new();
}
public sealed class VideoCourse : Product {public int Duration {get;set;}}
public sealed class ProductDetails
{
    public string Level {get;set;}="";
    public string Language {get;set;}="";
}
