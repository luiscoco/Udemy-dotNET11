using var embeddings = new EmbeddingService();
await using var store = await VectorStore.Open(205);
await store.CreateCollection(); await store.Seed(embeddings);
var query = await embeddings.Generate("rotate API keys", query: true);
var rankings = new Dictionary<string, Hit[]>();
foreach (string strategy in new[] { "vector", "keyword", "hybrid", "reranked" })
{
    var hits = await store.Search(query.Vector, "rotate API keys", strategy);
    rankings[strategy] = hits.ToArray();
    Console.WriteLine($"SEARCH {strategy}: {string.Join(",", hits.Select(h => h.Id))}");
    Check.That(hits.Count > 0 && hits.All(h => double.IsFinite(h.Score)), strategy + " executes a real supported database query");
}
var candidates = args.Contains("problem") ? Array.Empty<Hit>() : rankings["hybrid"];
bool retained = candidates.Any(h => h.Id == "keys" || h.Id == "security");
Console.WriteLine($"SEARCH hybridCandidates={candidates.Length} relevantRetained={retained}");
Check.That(retained, "candidate retrieval retains at least one explicitly relevant fixture document", "RAG candidate pool lost relevant documents");
AiRuntime.Evidence("rag-evidence.json", new { passed = true, rankings, reranker = "binary Hamming candidates followed by original-vector cosine", retained });
