using Microsoft.Extensions.AI;
public sealed record ProviderCapabilities(string Provider, bool Streaming, bool JsonSchema, bool Tools);
public static class CapabilityPolicy
{
    public static readonly ProviderCapabilities Ollama = new("OllamaSharp/Qwen2.5-0.5B", true, true, false);
    public static readonly ProviderCapabilities Native = new("LLamaSharp/Qwen2.5-0.5B", true, false, false);
    public static void RequireTools(ProviderCapabilities provider)
    {
        if (!provider.Tools)
            throw new NotSupportedException("Configured provider/model has no verified tool support.");
    }
    public static ChatOptions BoundedSettings() => new()
    {
        Temperature = 0, TopP = 0.9f, Seed = 42, MaxOutputTokens = 32
    };
}
