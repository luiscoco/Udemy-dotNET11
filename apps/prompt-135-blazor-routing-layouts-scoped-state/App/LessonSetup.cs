public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder) => builder.Services.AddScoped<CourseDraft>();
    public static void Map(WebApplication app) { }
}
