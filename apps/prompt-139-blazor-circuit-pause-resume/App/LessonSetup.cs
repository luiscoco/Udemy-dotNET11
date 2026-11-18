using Microsoft.AspNetCore.Components.Server.Circuits;
public static class LessonSetup
{
    public static void Add(WebApplicationBuilder builder)
    {
        builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
        {
            options.PersistedCircuitInMemoryRetentionPeriod = TimeSpan.FromSeconds(4);
            options.PersistedCircuitDistributedRetentionPeriod = TimeSpan.FromSeconds(4);
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(2);
        });
        builder.Services.AddScoped<TrackedCircuit>();
        builder.Services.AddScoped<CircuitHandler>(services => services.GetRequiredService<TrackedCircuit>());
    }
    public static void Map(WebApplication app) { }
}
