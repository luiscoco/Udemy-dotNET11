using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public sealed record ProductV1(int Id,string Name);
public sealed record ProductV2(int Id,string Name,string? Description);
public static class VersionedRoutes
{
    public static void Map(WebApplication app)
    {
        var versions=app.NewApiVersionSet().HasApiVersion(new ApiVersion(1,0))
            .HasApiVersion(new ApiVersion(2,0)).ReportApiVersions().Build();
        var products=app.MapGroup("/api/v{version:apiVersion}/products").WithApiVersionSet(versions);
        products.MapGet("/{id:int}",(int id)=>TypedResults.Ok(new ProductV1(id,"Course book")))
            .MapToApiVersion(new ApiVersion(1,0));
        products.MapGet("/{id:int}",(int id)=>TypedResults.Ok(new ProductV2(id,"Course book","Optional v2 detail")))
            .MapToApiVersion(new ApiVersion(2,0));
    }
}
