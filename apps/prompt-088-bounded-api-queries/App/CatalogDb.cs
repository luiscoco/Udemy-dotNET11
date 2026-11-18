using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
public sealed class Product { public int Id {get;set;} public string Name {get;set;}=""; }
public sealed record ProductDto(int Id,string Name);
public sealed record PageResult(int Page,int Size,int Total,ProductDto[] Items);
public sealed class CatalogDb(DbContextOptions<CatalogDb> options) : DbContext(options)
{ public DbSet<Product> Products => Set<Product>(); }
public sealed class QueryCounter : DbCommandInterceptor
{
    private int reads; public int Reads=>Volatile.Read(ref reads);
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,CommandEventData data,
        InterceptionResult<DbDataReader> result,CancellationToken ct=default)
    { Interlocked.Increment(ref reads); return new(result); }
}
