using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
string? previous = Environment.GetEnvironmentVariable("LCE74_Demo__Theme");
Environment.SetEnvironmentVariable("LCE74_Demo__Theme", "environment");
try
{
    Settings.Write("v1");
    using var json = Settings.Build(1);
    using var environment = Settings.Build(2);
    using var combined = Settings.Build(3, args.FirstOrDefault() == "order");
    Check.That(json["Demo:Theme"] == "json", "the JSON-only provider supplies its value");
    Check.That(environment["Demo:Theme"] == "environment", "the later prefixed environment provider overrides JSON");
    Console.WriteLine($"JSON={json["Demo:Theme"]}; environment={environment["Demo:Theme"]}; combined={combined["Demo:Theme"]}");
    Check.That(combined["Demo:Theme"] == "cli", "the last command-line provider wins the duplicate key");
    DisplaySettings snapshot = combined.GetSection("Demo").Get<DisplaySettings>()!;
    var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    using var subscription = ChangeToken.OnChange(combined.GetReloadToken, () => changed.TrySetResult());
    Settings.Write("v2");
    combined.Reload(); // Deterministic refresh; a watcher is also configured.
    await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Check.That(combined["Demo:Note"] == "v2" && combined["Demo:Theme"] == "cli", "reload updates the JSON value while the later CLI override remains");
    Console.WriteLine($"Reloaded Note={combined["Demo:Note"]}; previously bound Note={snapshot.Note}");
    if (args.FirstOrDefault() == "stale")
        Check.That(snapshot.Note == "v2", "a previously bound object does not update itself after reload");
    Check.That(snapshot.Note == "v1", "a plain bound object remains its original snapshot");
}
finally { Environment.SetEnvironmentVariable("LCE74_Demo__Theme", previous); }
