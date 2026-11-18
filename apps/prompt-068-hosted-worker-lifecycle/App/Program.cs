using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
bool fatal = args.FirstOrDefault() == "fatal";
var builder = Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
    options.ShutdownTimeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton(new Worker(fatal));
builder.Services.AddHostedService(provider => provider.GetRequiredService<Worker>());
using var host = builder.Build();
var worker = host.Services.GetRequiredService<Worker>();
var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() => worker.Events.Enqueue("host stopping"));
lifetime.ApplicationStopped.Register(() => worker.Events.Enqueue("host stopped"));
await host.StartAsync();
await worker.Ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check.That(worker.Completed == 0, "startup observes readiness without assuming work is already complete");
if (args.FirstOrDefault() == "stop")
{
    await host.StopAsync();
    await worker.Finished.Task.WaitAsync(TimeSpan.FromSeconds(10));
    try { await worker.ExecuteTask!; }
    catch (OperationCanceledException) when (lifetime.ApplicationStopping.IsCancellationRequested)
        { Console.WriteLine("Observed worker cancellation during its gated wait"); }
    foreach (string entry in worker.Events) Console.WriteLine(entry);
    Check.That(worker.Cleaned == 1 && worker.Completed == 0, "stopping cancels active waiting and completes cleanup");
    return;
}
worker.Release.SetResult();
try
{
    await worker.Finished.Task.WaitAsync(TimeSpan.FromSeconds(10));
    await worker.ExecuteTask!;
}
finally
{
    await host.StopAsync();
    foreach (string entry in worker.Events) Console.WriteLine(entry);
}
Check.That(worker.Completed == 2 && worker.Recovered == 1, "the declared recoverable failure is reported while other jobs complete");
Check.That(worker.Cleaned == 1 && worker.Events.Last() == "host stopped", "worker cleanup and host shutdown both complete");
