using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddSingleton<WorkState>();
builder.Services.AddHostedService<CourseWorker>();
using var host = builder.Build();
await host.StartAsync();
var state = host.Services.GetRequiredService<WorkState>();
if (args.FirstOrDefault() == "early")
{
    if (!state.Completed.Task.IsCompleted) throw new InvalidOperationException("incorrect expectation: host startup means background work has completed");
}
await state.Completed.Task.WaitAsync(TimeSpan.FromSeconds(10));
await host.StopAsync();
if (state.CompletedJobs != 2) throw new InvalidOperationException("Duplicate work was committed");
Console.WriteLine($"PASS worker processed {state.CompletedJobs} unique jobs and stopped gracefully");
Console.WriteLine($"service: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}, {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
public sealed class WorkState
{
    public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int CompletedJobs { get; set; }
}
public sealed class CourseWorker(WorkState state) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processed = new HashSet<string>();
        foreach (var id in new[]{"lesson-11","lesson-11","lesson-12"})
        {
            await Task.Delay(150,stoppingToken);
            if (processed.Add(id)) state.CompletedJobs++;
        }
        state.Completed.TrySetResult();
    }
}
