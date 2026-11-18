using Microsoft.Extensions.AI;
using System.Text.Json;
using var embeddings = new EmbeddingService();
await using var store = await VectorStore.Open(208);
await store.CreateCollection(); await store.Seed(embeddings);
using var chat = AiRuntime.Create();
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
var rewrite = await chat.GetResponseAsync<Rewrite>(
    "Return First and Second as two search phrases about API key rotation. Each phrase must contain at most six words.",
    options: AiRuntime.Options(128), useJsonSchemaResponseFormat: true, cancellationToken: deadline.Token);
const string original = "rotate API keys";
var generated = args.Contains("problem") ? Enumerable.Range(0,20).Select(i => "rotate API keys formulation " + i) : new[] { rewrite.Result.First, rewrite.Result.Second };
var plan = QueryPlanner.Plan(original, generated, !args.Contains("problem"));
var lists = new List<List<Hit>>();
foreach (string query in plan)
{
    var vector = await embeddings.Generate(query, query: true);
    lists.Add(await store.Search(vector.Vector, query));
}
Console.WriteLine($"REWRITE rewriteCalls=1 embeddingCalls={plan.Length} databaseSearches={store.Searches} budget=3 originalPreserved={plan[0] == original}");
Check.That(store.Searches <= 3, "rewriting stays inside the explicit search budget", "RAG query rewriting exceeded search budget");
Check.That(plan[0] == original && plan.All(q => q.Length <= 100), "the original query and bounded validated formulations are retained");
var fused = QueryPlanner.Fuse(lists);
Check.That(fused.Length > 0, "actual search results are combined by reciprocal rank fusion");
AiRuntime.Evidence("rag-evidence.json", new { passed = true, rewriteCalls = 1, embeddingCalls = plan.Length, searches = store.Searches, plan, original = lists[0].Select(h => h.Id), fused });
