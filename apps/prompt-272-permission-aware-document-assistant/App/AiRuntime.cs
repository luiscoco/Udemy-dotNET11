using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text.Json;

public static class AiRuntime
{
    public static string Endpoint => Environment.GetEnvironmentVariable("COURSE_AI_ENDPOINT") ?? "http://127.0.0.1:11439";

    public const string Model = "qwen2.5:0.5b";
    public static IChatClient Create() => new OllamaApiClient(new Uri(Endpoint), Model);
    public static ChatOptions Options(int tokens = 48) => new()
    {
        Temperature = 0,
        Seed = 42,
        MaxOutputTokens = tokens
    };
    public static async Task<string> Ask(IChatClient client, string prompt)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var response = await client.GetResponseAsync(prompt, Options(), timeout.Token);
        Check.That(!string.IsNullOrWhiteSpace(response.Text), "live inference returned text");
        Console.WriteLine("MODEL " + response.Text.Replace('\n', ' '));
        return response.Text;
    }

    public static void Evidence(string name, object value) => Harness.Save(name, value);
}
