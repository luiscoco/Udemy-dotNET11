using System.Text.Json;
var sanity = RankingMetrics.Measure(["keys"], new() { ["keys"] = 3, ["security"] = 2 }, 3, args.Contains("problem"));
Console.WriteLine($"METRIC controlledRecall={sanity.Recall:F3} expected=0.500");
Check.That(Math.Abs(sanity.Recall - 0.5) < 1e-9, "recall includes relevant documents absent from the ranking", "RAG recall denominator hides missed relevance");
var position = RankingMetrics.Measure(["unrelated","keys"], new() { ["keys"] = 3 }, 3);
Check.That(Math.Abs(position.Mrr - .5) < 1e-9 && Math.Abs(position.Ndcg - .6309297535714575) < 1e-9, "known rank-two fixture verifies reciprocal-rank and logarithmic discount");
using var embeddings = new EmbeddingService();
await using var store = await VectorStore.Open(210);
await store.CreateCollection(); await store.Seed(embeddings);
var questions = JsonSerializer.Deserialize<JudgedQuestion[]>(File.ReadAllText("judgments.json"))!;
var results = new List<Evaluation>();
foreach (var question in questions)
{
    var query = await embeddings.Generate(question.Question,query:true);
    foreach (string strategy in new[] { "vector", "keyword", "hybrid", "reranked" })
    {
        var hits = await store.Search(query.Vector,question.Question,strategy);
        var metrics = RankingMetrics.Measure(hits.Select(h=>h.Id).ToArray(),question.Relevance,3);
        Check.That(metrics.Recall is >= 0 and <= 1 && metrics.Ndcg is >= 0 and <= 1, "retrieval metrics remain in their defined bounds");
        results.Add(new(question.Question,strategy,hits.Select(h=>h.Id).ToArray(),metrics));
    }
}
foreach (var group in results.GroupBy(e=>e.Strategy))
    Console.WriteLine($"METRIC {group.Key} meanRecall@3={group.Average(e=>e.Metrics.Recall):F3} meanMRR={group.Average(e=>e.Metrics.Mrr):F3} meanNDCG@3={group.Average(e=>e.Metrics.Ndcg):F3}");
AiRuntime.Evidence("rag-evidence.json", new { passed = true, questions = questions.Length, evaluations = results.Count, k = 3, sanity, position, results });
public sealed record Evaluation(string Question,string Strategy,string[] Ids,Metrics Metrics);
