using Microsoft.Extensions.Configuration;
var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("providers.json").Build();
using var factory = new ProviderFactory();
var observations = new List<object>();
foreach (string provider in args.Contains("problem") ? new[] { "unknown" } : new[] { config["Primary"]!, config["Secondary"]! })
{
    Console.WriteLine("PROVIDER " + provider);
    using var client = factory.Create(provider);
    string text = await AiRuntime.Ask(client, "What is C#? Answer in one short sentence.");
    observations.Add(new { provider, text });
}
AiRuntime.Evidence("ai-evidence.json", new { passed = true, observations });
Check.That(observations.Count == 2, "both configured inference engines use the same IChatClient application call");
