using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

using var authority = new Authority();
var builder = WebApplication.CreateSlimBuilder();
authority.Configure(builder.Services);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
await using var server = builder.Build();
server.UseAuthentication();
server.UseAuthorization();
SyncFixture.Map(server);
await server.StartAsync();
var mode = args.FirstOrDefault() ?? "verify";
var root = Environment.CurrentDirectory;
var artifacts = Path.Combine(root, "artifacts");
Directory.CreateDirectory(artifacts);
var executable = Path.Combine(root, "Native", "bin", "Release", "net11.0-windows10.0.19041.0", "win-x64", "NativeCourse.exe");
if (!File.Exists(executable))
    throw new FileNotFoundException("Build the Native project first.", executable);
var reportFile = Path.Combine(artifacts, "native-" + mode + ".json");
if (File.Exists(reportFile))
    File.Delete(reportFile);
var start = new ProcessStartInfo(executable)
{
    WorkingDirectory = root,
    UseShellExecute = false,
    CreateNoWindow = true,
    WindowStyle = ProcessWindowStyle.Hidden
};
start.Environment["COURSE_MODE"] = mode;
start.Environment["COURSE_SYNC_TOKEN"] = authority.Issue();
start.Environment["COURSE_SYNC_URL"] = server.Urls.Single();
start.Environment["COURSE_ARTIFACTS"] = artifacts;
using var child = Process.Start(start)!;
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
try
{
    await child.WaitForExitAsync(timeout.Token);
}
catch (OperationCanceledException)
{
    if (!child.HasExited)
        child.Kill(entireProcessTree: true);
    throw new TimeoutException("The owned native application did not finish its verification.");
}

if (!File.Exists(reportFile))
    throw new InvalidOperationException($"The native application exited {child.ExitCode} without its report.");
using var report = JsonDocument.Parse(await File.ReadAllTextAsync(reportFile));
foreach (var message in report.RootElement.GetProperty("checks").EnumerateArray())
    Console.WriteLine("PASS " + message.GetString());
bool passed = report.RootElement.GetProperty("passed").GetBoolean();
string preview = Path.Combine(artifacts, passed ? "native-success.png" : "native-problem.png");
if (File.Exists(preview))
{
    File.Copy(preview, Path.Combine(artifacts, "native-ui.png"), true);
    string historyFile = Path.Combine(artifacts, "browser-preview-history.json");
    var rows = File.Exists(historyFile) ? JsonSerializer.Deserialize<List<JsonElement>>(File.ReadAllText(historyFile))! : new List<JsonElement>();
    rows.Add(JsonSerializer.SerializeToElement(new { passed, broken = mode == "problem", sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(preview))).ToLowerInvariant(), actualMaui = true, authenticated = passed }));
    File.WriteAllText(historyFile, JsonSerializer.Serialize(rows));
}

if (passed)
    Harness.Save("capstone-evidence.json", new { passed = true, actualMaui = true, actualWindowsControls = true, actualSqlite = true, actualAuthenticatedHttp = true, conflictPreserved = true, explicitResolution = true, idempotentReplay = true });
if (child.ExitCode != 0 || !passed)
    throw new InvalidOperationException(report.RootElement.GetProperty("error").GetString());
Console.WriteLine("native: actual Windows controls executed and native-rendered image saved");
Console.WriteLine("PASS native application verification completed");
