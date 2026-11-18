using System.Diagnostics;
using System.Text.Json;
using var compatibility = JsonDocument.Parse(await File.ReadAllTextAsync("hosting-support.json"));
var root = compatibility.RootElement;
Check.That(root.GetProperty("target").GetString() == "net11.0", "the recorded target is .NET 11");
Console.WriteLine("service: provider support checked on " + root.GetProperty("checkedOn").GetString());
Console.WriteLine("service: " + root.GetProperty("providerModel").GetString());
await DockerAsync(["build","-t","dotnet-course-160-worker:local","."]);
await DockerAsync(["run","--rm","--network","none","dotnet-course-160-worker:local",args.FirstOrDefault() ?? "verify"]);
Console.WriteLine("PASS the published .NET 11 worker executed inside its Linux container");
static async Task DockerAsync(string[] arguments)
{
    var start = new ProcessStartInfo("docker") { UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardError=true, CreateNoWindow=true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    using var child = Process.Start(start)!;
    var output = child.StandardOutput.ReadToEndAsync();
    var errors = child.StandardError.ReadToEndAsync();
    using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
    try { await child.WaitForExitAsync(deadline.Token); }
    catch (OperationCanceledException) { child.Kill(true); throw; }
    Console.Write(await output);
    Console.Write(await errors);
    if (child.ExitCode != 0) throw new InvalidOperationException("Container workflow failed with exit " + child.ExitCode);
}
