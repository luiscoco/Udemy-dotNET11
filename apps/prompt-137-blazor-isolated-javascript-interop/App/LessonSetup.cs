public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder) => builder.Services.AddSingleton<InteropCounts>();
    public static void Map(WebApplication app) => app.MapGet("/interop-counts", (InteropCounts counts) => new { counts.Disposed });
}
