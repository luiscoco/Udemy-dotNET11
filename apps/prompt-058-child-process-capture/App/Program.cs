using System.Diagnostics;
using System.Text.Json;
if (args.FirstOrDefault() == "child")
{
    if (args.Contains("wait")) { await Task.Delay(30_000); return; }
    Console.Out.WriteLine(JsonSerializer.Serialize(args.Skip(1).ToArray()));
    Console.Error.WriteLine("diagnostic from child");
    Environment.ExitCode = args.Contains("fail") ? 7 : 0;
    return;
}
if (args.FirstOrDefault() == "cancel")
{
    using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
    _ = await ChildRunner.CaptureAsync(["wait"], source.Token);
    return;
}
var output = await ChildRunner.CaptureAsync(["folder with spaces", "literal;&value"]);
Console.WriteLine($"stdout: {output.StandardOutput.Trim()}");
Console.WriteLine($"stderr: {output.StandardError.Trim()}");
var echoed = JsonSerializer.Deserialize<string[]>(output.StandardOutput)!;
Check.That(echoed.SequenceEqual(new[] { "folder with spaces", "literal;&value" }), "ArgumentList preserves two literal arguments without a shell");
Check.That(output.StandardError.Contains("diagnostic") && output.ExitStatus.ExitCode == 0,
    "stdout, stderr and successful status are captured independently");
var failed = await ChildRunner.CaptureAsync(["fail"]);
Console.WriteLine($"Controlled child exit code {failed.ExitStatus.ExitCode}");
if (args.FirstOrDefault() == "ignore")
    Check.That(failed.ExitStatus.ExitCode == 0, "expected process start to guarantee command success");
else
    Check.That(failed.ExitStatus.ExitCode == 7, "the caller observes and handles the nonzero status");
