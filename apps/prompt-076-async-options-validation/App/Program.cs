using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
string mode = args.FirstOrDefault() ?? "verify";
Directory.CreateDirectory("artifacts");
string marker = Path.GetFullPath("artifacts/validation-marker.txt");
await File.WriteAllTextAsync(marker, mode == "invalid" ? "NOT-READY" : "READY");
var probe = new ValidationProbe(mode == "cancel");
var builder = Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();
builder.Services.AddSingleton(probe);
builder.Services.AddOptions<EndpointOptions>().Configure(options => options.MarkerPath = marker)
    .Validate<MarkerValidator>()
    .ValidateOnStart();
using var host = builder.Build();
if (mode == "early")
{
    _ = host.Services.GetRequiredService<IOptionsMonitor<EndpointOptions>>().CurrentValue;
    return;
}
using var cancellation = new CancellationTokenSource();
Task startup = host.StartAsync(cancellation.Token);
if (mode == "cancel")
{
    await probe.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
    cancellation.Cancel();
}
try { await startup; }
catch (OptionsValidationException error)
{
    foreach (string message in error.Failures) Console.WriteLine("VALIDATION " + message);
    throw;
}
try
{
    var ready = host.Services.GetRequiredService<IOptionsMonitor<EndpointOptions>>().CurrentValue;
    Check.That(ready.MarkerPath == marker && !probe.SawCancellation,
        "asynchronous validation completes before startup exposes the validated monitor value");
    Console.WriteLine("Startup completed after the real asynchronous file validation.");
}
finally { await host.StopAsync(); }
