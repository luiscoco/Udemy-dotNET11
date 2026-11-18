using Microsoft.Extensions.AI;
bool broken = args.Contains("problem");
var store = new ConversationStore();
for (int turn = 0; turn < 3; turn++)
{
    store.Add("alice", ChatRole.User, $"Alice marker ALICE_ONLY turn {turn}");
    store.Add("alice", ChatRole.Assistant, "Acknowledged.");
}
store.Add("bob", ChatRole.User, "Explain bounded context briefly.");
var outgoing = store.Context(broken ? "alice" : "bob");
bool isolated = outgoing.All(m => !m.Text.Contains("ALICE_ONLY"));
Console.WriteLine($"HISTORY requested=bob isolated={isolated} messages={outgoing.Length}");
AiRuntime.Evidence("history-boundary.json", new { isolated, roles = outgoing.Select(m => m.Role.Value), texts = outgoing.Select(m => m.Text) });
Check.That(isolated, "outgoing context excludes other-user data", "AI cross-user history detected");
Check.That(store.Context("alice").Length == 5, "oldest complete turn removed; system plus four messages remain");
Check.That(outgoing[0].Role == ChatRole.System && outgoing[1].Role == ChatRole.User, "explicit roles retained");
using var client = AiRuntime.Create();
var response = await client.GetResponseAsync(outgoing, AiRuntime.Options());
Console.WriteLine("MODEL " + response.Text.Replace('\n', ' '));
Check.That(response.Text.Length > 0, "bounded isolated context reaches actual model");
AiRuntime.Evidence("ai-evidence.json", new { passed = true, isolated, response = response.Text, bounded = true });
