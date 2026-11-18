using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;
var builder = Host.CreateApplicationBuilder(args);
builder.AddNpgsqlDataSource("coursedb");
builder.AddRedisClient("cache");
builder.Services.AddHostedService<CourseWorker>();
await builder.Build().RunAsync();
public sealed class CourseWorker(NpgsqlDataSource database, IConnectionMultiplexer cache) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var command = database.CreateCommand("CREATE TABLE IF NOT EXISTS course_demo(id integer PRIMARY KEY,title text NOT NULL); INSERT INTO course_demo VALUES(11,'Background worker completed') ON CONFLICT(id) DO UPDATE SET title=excluded.title;");
        await command.ExecuteNonQueryAsync(stoppingToken);
        await cache.GetDatabase().StringSetAsync("course:11", "Background worker completed");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
