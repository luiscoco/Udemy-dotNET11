using Contracts;
using Microsoft.Extensions.DependencyInjection;
namespace Catalog;
public static class CatalogModule
{
    public static IServiceCollection AddCatalog(this IServiceCollection services)
        => services.AddSingleton<ICourseCatalog, CourseStore>();
}
internal sealed class CourseStore : ICourseCatalog
{
    public CourseSummary? Find(int id)
        => id == 11 ? new(id, ".NET architecture") : null;
}
