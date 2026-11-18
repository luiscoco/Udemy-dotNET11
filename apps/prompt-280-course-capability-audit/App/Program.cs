using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Contains("audit-child"))
{
    Console.WriteLine("API-280");
    return;
}

bool broken = args.Contains("problem");
var inventory = InventoryAudit.Run();
var sources = JsonDocument.Parse(File.ReadAllText("assets/documentation-sources.json"));
foreach (var source in sources.RootElement.EnumerateArray())
{
    string file = source.GetProperty("file").GetString()!;
    string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
    Check.That(hash == source.GetProperty("sha256").GetString(), "retained official documentation hash matches " + Path.GetFileName(file));
}

var additions = await ApiAdditions.Verify();
// Inspect metadata without opting this audit into using the experimental API itself.
var hpke = typeof(SHA256).Assembly.GetType("System.Security.Cryptography.Hpke", throwOnError: true)!;
var attribute = hpke.GetCustomAttributesData().Single(a => a.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute");
string diagnostic = (string)attribute.ConstructorArguments[0].Value!;
string classification = broken ? "stable" : "experimental";
Console.WriteLine("AUDIT HPKE actualDiagnostic=" + diagnostic + " manifestClassification=" + classification);
Check.That(classification == "experimental" && diagnostic == "SYSLIB5009", "API status follows actual experimental metadata", "CAPSTONE audit experimental classification failed");
Harness.Save("capstone-evidence.json", new { passed = true, officialSources = sources.RootElement.Clone(), inventory, additions, hpkeExperimental = true, diagnostic, scope = "bounded evidence inventory and four verified supplemental APIs; not an exhaustive semantic audit or certification of every exercise" });
