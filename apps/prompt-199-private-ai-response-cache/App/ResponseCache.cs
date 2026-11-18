using Microsoft.Extensions.AI;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
public sealed class ResponseCache(IChatClient chat, bool scopeUsers)
{
    private readonly Dictionary<string, (DateTimeOffset Expires, string Text)> cache = [];
    public int ModelCalls { get; private set; }
    public int Entries => cache.Count;
    public async Task<string> Get(string user, string prompt, bool sensitive = false)
    {
        string identity = JsonSerializer.Serialize(new
        { user = scopeUsers ? user : "shared", model = AiRuntime.Model, template = "v1", temperature = 0, prompt });
        string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
        if (!sensitive && cache.TryGetValue(key, out var hit) && hit.Expires > DateTimeOffset.UtcNow)
            return hit.Text;
        ModelCalls++;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var response = await chat.GetResponseAsync(prompt, AiRuntime.Options(), deadline.Token);
        string text = response.Text;
        Check.That(!string.IsNullOrWhiteSpace(text), "model response generated without logging content");
        if (!sensitive) cache[key] = (DateTimeOffset.UtcNow.AddMinutes(1), text);
        return text;
    }
}
