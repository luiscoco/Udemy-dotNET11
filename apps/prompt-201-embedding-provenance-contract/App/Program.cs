using System.Text.Json;
using var embeddings = new EmbeddingService();
var first = await embeddings.Generate("  Rotate API keys\n every 30 days.  ");
var same = await embeddings.Generate("Rotate API keys every 30 days.");
var declared = first.Stamp with { Dimensions = args.Contains("problem") ? 768 : first.Vector.Length };
Console.WriteLine($"EMBED model={declared.Model} dimensions={first.Vector.Length} declared={declared.Dimensions}");
Check.That(declared.Dimensions == first.Vector.Length,
    "declared dimensions match the actual model output", "RAG embedding dimension metadata mismatch");
Check.That(first.ContentHash == same.ContentHash, "normalization produces a stable content identity");
Check.That(first.Stamp.Digest.Length == 64 && first.Stamp.Version.Length > 0, "model digest and schema version are recorded");
double difference = first.Vector.Zip(same.Vector).Max(pair => Math.Abs(pair.First - pair.Second));
Console.WriteLine($"EMBED normalizedHash={first.ContentHash[..12]} repeatedDifference={difference:F8}");
AiRuntime.Evidence("rag-evidence.json", new { passed = true, first.Stamp, first.ContentHash, difference, actualDimensions = first.Vector.Length });
