using System.Text.Json;

bool broken = args.Contains("problem");
var results = new List<JsonElement>();
var deployments = new List<object>();
foreach (string framework in new[]
{
    "net10.0",
    "net11.0"
}

)
{
    string directory = Path.GetFullPath("artifacts/published/" + framework);
    var published = await Harness.Run(Harness.Dotnet, "publish", "Worker", "-c", "Release", "-f", framework, "-r", "win-x64", "--self-contained", "true", "-p:UseAppHost=true", "-o", directory);
    Check.That(published.Code == 0, "self-contained " + framework + " publishes", published.Text);
    var executed = await Harness.Run(Path.Combine(directory, "Worker.exe"), broken ? "problem" : "verify");
    Check.That(executed.Code == 0, "published worker actually executes", executed.Text);
    var result = JsonDocument.Parse(executed.Text).RootElement.Clone();
    results.Add(result);
    long bytes = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
    int files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length;
    deployments.Add(new { framework, bytes, files, actualSelfContained = true });
    Console.WriteLine($"UPGRADE {framework} runtime={result.GetProperty("runtime")} total={result.GetProperty("total")} medianMs={result.GetProperty("medianMs")} deploymentBytes={bytes}");
}

Check.That(results[0].GetProperty("runtime").GetString()!.StartsWith(".NET 10.") && results[1].GetProperty("runtime").GetString()!.StartsWith(".NET 11."), "actual published deployments use different major runtimes");
Check.That(results.All(r => r.GetProperty("total").GetDecimal() == 37.50m), "JSON contract and decimal result survive the upgrade", "CAPSTONE upgrade JSON contract failed");
Check.That(results.All(r => r.GetProperty("samples").GetArrayLength() == 5), "five measured post-warmup samples exist for each actual runtime");
// Illustrative capacity model: supplied rates, not a cloud quotation or observed bill.
const double requests = 1_000_000, pricePerCpuHour = 0.05;
var cost = results.Select(r => new { runtime = r.GetProperty("runtime").GetString(), estimatedCpuHours = r.GetProperty("medianMs").GetDouble() / 1000 / 5000 * requests / 3600, illustrativeComputeUsd = r.GetProperty("medianMs").GetDouble() / 1000 / 5000 * requests / 3600 * pricePerCpuHour }).ToArray();
Harness.Save("capstone-evidence.json", new { passed = true, actualTwoRuntimes = true, correctTotal = 37.50m, results, deployments, cost, assumptions = new { requests, pricePerCpuHour, scope = "synthetic local JSON loop, elapsed time used as CPU proxy; excludes IO, hosting, idle capacity, storage, transfer and support; no general speedup or cost saving claim" } });
