using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
public static class QueryRoutes
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/products",ReadPage);
        app.MapGet("/products/after",async (int? afterId,int? size,CatalogDb db,CancellationToken ct) =>
        {
            int count=size??10; int last=afterId??0;
            if(last<0 || count<1 || count>20) return Results.BadRequest(new {Error="Invalid keyset bounds"});
            var rows=await db.Products.AsNoTracking().Where(p=>p.Id>last).OrderBy(p=>p.Id)
                .Take(count).Select(p=>new ProductDto(p.Id,p.Name)).ToArrayAsync(ct);
            return Results.Ok(rows);
        });
    }
    private static async Task<IResult> ReadPage(int? page,int? size,string? title,string? sort,CatalogDb db,CancellationToken ct)
    {
        int number=page??1,count=size??10; string order=sort??"id";
        if(number<1 || number>100 || count<1 || count>20 || title?.Length>40 || order is not ("id" or "name"))
            return Results.BadRequest(new {Error="Page 1..100, size 1..20, title <=40, sort id|name"});
        IQueryable<Product> query=db.Products.AsNoTracking();
        if(!string.IsNullOrEmpty(title)) query=query.Where(p=>p.Name.Contains(title));
        int total=await query.CountAsync(ct);
        var ordered=order=="name" ? query.OrderBy(p=>p.Name).ThenBy(p=>p.Id) : query.OrderBy(p=>p.Id);
        var rows=await ordered.Skip((number-1)*count).Take(count)
            .Select(p=>new ProductDto(p.Id,p.Name)).ToArrayAsync(ct);
        return Results.Ok(new PageResult(number,count,total,rows));
    }
}
