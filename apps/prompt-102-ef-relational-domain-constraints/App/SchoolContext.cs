using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public sealed class SchoolContext(DbContextOptions<SchoolContext> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.ApplyConfiguration(new CourseConfiguration());
        model.Entity<Enrollment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Student).IsRequired();
            e.HasIndex(x => new { x.CourseId, x.Student }).IsUnique();
            e.HasOne(x => x.Course).WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Title).IsRequired().HasMaxLength(120);
        entity.HasIndex(x => x.Title).IsUnique();
        entity.Property(x => x.Revision).IsConcurrencyToken();
        entity.ToTable("Courses", t => t.HasCheckConstraint("CK_Seats", "Seats >= 0"));
    }
}
