using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
bool broken = args.Contains("problem");
var services = new ServiceCollection();
if (!broken)
    services.AddSingleton<IChatClient>(_ => AiRuntime.Create());
services.AddTransient<ChatApplication>();
using var provider = services.BuildServiceProvider();
try
{
    var app = provider.GetRequiredService<ChatApplication>();
    string text = await app.Answer("Explain dependency injection in one sentence.");
    AiRuntime.Evidence("ai-evidence.json", new { passed = true, provider = "OllamaSharp", model = AiRuntime.Model, text });
    Check.That(text.Length > 0, "application consumes IChatClient through DI");
}
catch (InvalidOperationException e) when (broken)
{
    Console.WriteLine("AI missing IChatClient registration: " + e.Message);
    AiRuntime.Evidence("controlled-failure.json", new { passed = true, diagnostic = e.Message });
    throw;
}
public sealed class ChatApplication(IChatClient chat)
{
    public Task<string> Answer(string question) => AiRuntime.Ask(chat, question);
}
