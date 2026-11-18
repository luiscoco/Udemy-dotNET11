using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public sealed class InventoryDb(DbContextOptions<InventoryDb> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>().HasKey(x => x.Sku);
        b.Entity<Receipt>().HasKey(x => x.OperationId);
    }
}

public sealed class Product
{
    public string Sku { get; set; } = "book";
    public int Available { get; set; } = 8;
}

public sealed class Receipt
{
    public string OperationId { get; set; } = "";
    public string Subject { get; set; } = "";
    public int Quantity { get; set; }
    public int Remaining { get; set; }
}

public sealed record Reservation([Range(1, 100)] int Quantity, [Required] string OperationId);
