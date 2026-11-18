using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
Directory.CreateDirectory("artifacts"); string file=Path.GetFullPath("artifacts/catalog.db"); var counter=new QueryCounter();
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddDbContext<CatalogDb>(o=>o.UseSqlite("Data Source="+file).AddInterceptors(counter)),QueryRoutes.Map);
await using(var scope=api.App.Services.CreateAsyncScope())
{
    var db=scope.ServiceProvider.GetRequiredService<CatalogDb>(); await db.Database.EnsureDeletedAsync(); await db.Database.EnsureCreatedAsync();
    db.Products.AddRange(Enumerable.Range(1,30).Select(i=>new Product {Id=i,Name=(i<=12?"Book ":"Tool ")+i.ToString("D2")})); await db.SaveChangesAsync();
}
var page=(await api.Client.GetFromJsonAsync<PageResult>("/products?page=2&size=3&title=Book&sort=name"))!;
Check.That(page.Total==12 && page.Items.Select(p=>p.Id).SequenceEqual(new[]{4,5,6}),"bounded filtering and stable ordering return the intended second page");
Console.WriteLine("database: page 2 IDs "+string.Join(",",page.Items.Select(p=>p.Id))+"; filtered total "+page.Total);
foreach(string query in new[]{"size=1000000","page=0","sort=arbitrary","title="+new string('x',41)})
{
    int before=counter.Reads; using var invalid=await api.Client.GetAsync("/products?"+query);
    if(args.FirstOrDefault()=="bounds") Check.That((int)invalid.StatusCode==200,"oversized query input must be rejected before database execution");
    Check.That((int)invalid.StatusCode==400 && counter.Reads==before,"invalid query bounds perform no database read");
}
var next=(await api.Client.GetFromJsonAsync<ProductDto[]>("/products/after?afterId=5&size=3"))!;
Check.That(next.Select(p=>p.Id).SequenceEqual(new[]{6,7,8}),"keyset pagination continues after the stable unique ID");
Console.WriteLine("database: keyset IDs "+string.Join(",",next.Select(p=>p.Id)));
