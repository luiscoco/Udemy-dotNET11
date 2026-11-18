using Microsoft.Extensions.AI;
if (args.Contains("problem"))
{
    Console.WriteLine("CAPABILITY unsupported tools requested from local small model");
    try { CapabilityPolicy.RequireTools(CapabilityPolicy.Ollama); }
    catch (NotSupportedException e) { throw new InvalidOperationException("AI unsupported capability requested", e); }
}
using var factory = new ProviderFactory();
var observations = new List<object>();
foreach (var settings in new[] { ("ollama", CapabilityPolicy.Ollama), ("llamasharp", CapabilityPolicy.Native) })
{
    using var client = factory.Create(settings.Item1);
    var options = CapabilityPolicy.BoundedSettings();
    int updates = 0; string text = "";
    await foreach (var update in client.GetStreamingResponseAsync("Name one benefit of C# briefly.", options))
    {
        if (update.Text.Length > 0) { updates++; text += update.Text; }
    }
    Console.WriteLine($"CAPABILITY {settings.Item1} textUpdates={updates} schema={settings.Item2.JsonSchema} tools={settings.Item2.Tools}");
    Check.That(updates > 0 && text.Length > 0, "configured provider actually streams with bounded settings");
    observations.Add(new { provider = settings.Item1, updates, text, policy = settings.Item2 });
}
using var schemaClient = AiRuntime.Create();
var typed = await schemaClient.GetResponseAsync<Fact>("Return topic CSharp and count 1.", options: AiRuntime.Options(40), useJsonSchemaResponseFormat: true);
Check.That(!string.IsNullOrWhiteSpace(typed.Result.Topic), "Ollama JSON schema support exercised independently");
AiRuntime.Evidence("ai-evidence.json", new { passed = true, observations, schema = typed.Result, reproducibleTextClaimed = false });
public sealed record Fact(string Topic, int Count);
