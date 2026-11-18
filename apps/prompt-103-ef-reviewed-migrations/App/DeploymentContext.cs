using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
public sealed class DeploymentContext(DbContextOptions<DeploymentContext> options) : DbContext(options)
{
    public DbSet<DeployCourse> Courses=>Set<DeployCourse>();
    protected override void OnModelCreating(ModelBuilder model)
    {model.Entity<DeployCourse>().ToTable("Courses");}
}
public sealed class DeployCourse
{
    public int Id {get;set;}
    public string Title {get;set;}="";
    public int Seats {get;set;}
}
public sealed class DesignFactory : IDesignTimeDbContextFactory<DeploymentContext>
{
    public DeploymentContext CreateDbContext(string[] args)=>new(
        new DbContextOptionsBuilder<DeploymentContext>().UseSqlite("Data Source=design.db").Options);
}


