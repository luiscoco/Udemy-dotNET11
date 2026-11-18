using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
public sealed record EmbeddingStamp(string Model, string Digest, int Dimensions, string Preprocessing, string Version);
public sealed record EmbeddedText(string Text, float[] Vector, EmbeddingStamp Stamp, string ContentHash);
public sealed class EmbeddingService(string model = "all-minilm") : IDisposable
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> generator =
        new OllamaApiClient(new Uri(AiRuntime.Endpoint), model);
    public static string Normalize(string text) => Regex.Replace(text.Normalize(NormalizationForm.FormKC), @"\s+", " ").Trim();
    public async Task<EmbeddedText> Generate(string text, bool query = false)
    {
        string normalized = Normalize(text);
        string prefix = model.StartsWith("nomic") ? (query ? "search_query: " : "search_document: ") : "";
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var generated = await generator.GenerateAsync([prefix + normalized], cancellationToken: deadline.Token);
        float[] vector = generated[0].Vector.ToArray();
        Check.That(vector.Length > 0 && vector.All(float.IsFinite), "actual embedding contains finite values");
        using var http = new HttpClient();
        using var tags = JsonDocument.Parse(await http.GetStringAsync(AiRuntime.Endpoint + "/api/tags", deadline.Token));
        var selected = tags.RootElement.GetProperty("models").EnumerateArray()
            .First(m => m.GetProperty("name").GetString()!.Split(':')[0] == model.Split(':')[0]);
        string digest = selected.GetProperty("digest").GetString()!;
        var stamp = new EmbeddingStamp(model, digest, vector.Length,
            "NFKC; collapse whitespace; model-specific query/document prefix", "embedding-schema-v1");
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
        return new(normalized, vector, stamp, hash);
    }
    public void Dispose() => generator.Dispose();
}
