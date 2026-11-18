using System.Diagnostics;
using System.Text.Json;
var mode = args.FirstOrDefault() ?? "verify";
var root = Environment.CurrentDirectory;
var artifacts = Path.Combine(root, "artifacts");
Directory.CreateDirectory(artifacts);
var executable = Path.Combine(root, "Native", "bin", "Release", "net11.0-windows10.0.19041.0", "win-x64", "NativeCourse.exe");
if (!File.Exists(executable)) throw new FileNotFoundException("Build the Native project first.", executable);
var reportFile = Path.Combine(artifacts, "native-" + mode + ".json");
if (File.Exists(reportFile)) File.Delete(reportFile);
var start = new ProcessStartInfo(executable)
{
    WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true
};
start.Environment["COURSE_MODE"] = mode;
start.Environment["COURSE_ARTIFACTS"] = artifacts;
using var child = Process.Start(start)!;
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
try { await child.WaitForExitAsync(timeout.Token); }
catch (OperationCanceledException)
{
    if (!child.HasExited) child.Kill(entireProcessTree: true);
    throw new TimeoutException("The owned native application did not finish its verification.");
}
if (!File.Exists(reportFile)) throw new InvalidOperationException($"The native application exited {child.ExitCode} without its report.");
using var report = JsonDocument.Parse(await File.ReadAllTextAsync(reportFile));
foreach (var message in report.RootElement.GetProperty("checks").EnumerateArray())
    Console.WriteLine("PASS " + message.GetString());
if (child.ExitCode != 0 || !report.RootElement.GetProperty("passed").GetBoolean())
    throw new InvalidOperationException(report.RootElement.GetProperty("error").GetString());
Console.WriteLine("native: actual Windows controls executed and native-rendered image saved");
Console.WriteLine("PASS native application verification completed");
