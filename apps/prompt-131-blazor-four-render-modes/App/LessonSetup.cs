public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder) => builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
    public static void Map(WebApplication app) { }
}
