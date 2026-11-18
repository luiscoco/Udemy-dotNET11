using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlTypes;
public sealed class VectorDb(string table):DbContext
{
    public DbSet<VectorRow> Rows=>Set<VectorRow>();
    protected override void OnConfiguring(DbContextOptionsBuilder options)=>options.UseSqlServer("Server=127.0.0.1,14666;Database=CoursePreview266;User ID=sa;Password=Course266_Local!Only;Encrypt=True;TrustServerCertificate=True");
    protected override void OnModelCreating(ModelBuilder model){model.Entity<VectorRow>().ToTable(table,"dbo");model.Entity<VectorRow>().Property(r=>r.Embedding).HasColumnType("vector(3)");}
}
public sealed class VectorRow{public int Id{get;set;}public SqlVector<float> Embedding{get;set;}}
