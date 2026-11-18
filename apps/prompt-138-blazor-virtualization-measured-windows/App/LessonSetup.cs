public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder) => builder.Services.AddSingleton<ListMetrics>();
    public static void Map(WebApplication app) => app.MapGet("/metrics", (ListMetrics metrics) => new { metrics.Requests, metrics.Items });
}
