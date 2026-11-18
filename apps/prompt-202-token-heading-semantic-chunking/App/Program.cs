var chunker = new Chunker();
using var embeddings = new EmbeddingService();
var collection = Corpus.Documents.Where(d => d.Tenant == "a" && d.User == "alice")
    .Select(d => new Document(d.Id,d.Title,"# " + d.Title + "\n" + d.Body + " " + d.Body + "\n## Operational example\n" + d.Body + " " + d.Body)).ToArray();
var tokenChunks = collection.SelectMany(d => chunker.Bounded(d.Body)).ToArray();
var headingChunks = collection.SelectMany(d => System.Text.RegularExpressions.Regex.Split(d.Body,@"(?m)(?=^#{1,2} )")
    .Where(section => !string.IsNullOrWhiteSpace(section)).SelectMany(section => chunker.Bounded(section,48))).ToArray();
var semantic = new List<string>(); var boundaries = new List<double>();
foreach (var document in collection)
{
    string[] sentences = document.Body.Split(". ", StringSplitOptions.RemoveEmptyEntries);
    var vectors = new List<float[]>();
    foreach (string sentence in sentences) vectors.Add((await embeddings.Generate(sentence)).Vector);
    string current = sentences[0];
    for (int i = 1; i < sentences.Length; i++)
    {
        double similarity = Chunker.Cosine(vectors[i-1], vectors[i]); boundaries.Add(similarity);
        if (similarity < 0.55 || chunker.Tokens(current + ". " + sentences[i]) > 48)
        { semantic.Add(current); current = sentences[i]; }
        else current += ". " + sentences[i];
    }
    semantic.AddRange(chunker.Bounded(current));
}
string dense = string.Join(' ', Enumerable.Repeat("identifier_1234567890", 30));
var observed = args.Contains("problem") ? new[] { dense } : chunker.Bounded(dense).ToArray();
Console.WriteLine($"CHUNK token={tokenChunks.Length} heading={headingChunks.Length} semantic={semantic.Count} maxProbeTokens={observed.Max(chunker.Tokens)} budget=48");
Check.That(observed.All(c => chunker.Tokens(c) <= 48), "every probe chunk respects the tokenizer budget", "RAG chunk exceeds token budget");
Check.That(tokenChunks.Concat(headingChunks).Concat(semantic).All(c => chunker.Tokens(c) <= 48), "all three strategies respect the same hard token cap");
AiRuntime.Evidence("rag-evidence.json", new { passed = true, tokenizer = "BERT WordPiece / all-MiniLM vocabulary", tokenChunks, headingChunks, semantic, boundaries, budget = 48 });
