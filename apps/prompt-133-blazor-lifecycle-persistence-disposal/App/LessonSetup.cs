public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder) => builder.Services.AddSingleton<LifecycleStore>();
    public static void Map(WebApplication app) => app.MapGet("/counts", (LifecycleStore store) =>
        new { store.Loads, store.Renders, store.Disposed });
}
