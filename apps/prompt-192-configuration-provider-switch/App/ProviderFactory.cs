using Microsoft.Extensions.AI;
using LLama;
using LLama.Abstractions;
using LLama.Common;
public sealed class ProviderFactory : IDisposable
{
    private LLamaWeights? weights;
    public IChatClient Create(string provider)
    {
        return provider switch
        {
            "ollama" => AiRuntime.Create(),
            "llamasharp" => NativeClient(),
            _ => throw new ArgumentException("AI unsupported configured provider: " + provider)
        };
    }
    private IChatClient NativeClient()
    {
        string file = Environment.GetEnvironmentVariable("COURSE_AI_GGUF")
            ?? FindModel();
        var settings = new ModelParams(file) { ContextSize = 512, GpuLayerCount = 0, Threads = 2 };
        weights = LLamaWeights.LoadFromFile(settings);
        return new StatelessExecutor(weights, settings).AsChatClient();
    }
    private static string FindModel()
    {
        for (var folder = new DirectoryInfo(Environment.CurrentDirectory); folder != null; folder = folder.Parent)
        {
            string file = Path.Combine(folder.FullName, ".tools", "ai", "models", "blobs",
                "sha256-c5396e06af294bd101b30dce59131a76d2b773e76950acc870eda801d3ab0515");
            if (File.Exists(file)) return file;
        }
        throw new FileNotFoundException("Set COURSE_AI_GGUF to the Qwen2.5 0.5B GGUF model file.");
    }
    public void Dispose() => weights?.Dispose();
}
