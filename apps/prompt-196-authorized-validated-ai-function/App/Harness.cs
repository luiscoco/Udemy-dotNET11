using System.Diagnostics;
using System.Text.Json;
public static class Harness
{
    public static string Dotnet => Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
    public static Process Start(string file, IEnumerable<string> arguments, IReadOnlyDictionary<string,string>? environment = null)
    {
        var start = new ProcessStartInfo(file) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ROLL_FORWARD"] = "LatestMajor";
        foreach (var pair in environment ?? new Dictionary<string,string>()) start.Environment[pair.Key] = pair.Value;
        return Process.Start(start) ?? throw new InvalidOperationException("Could not start the owned process.");
    }
    public static async Task<(int Code, string Text)> Run(string file, params string[] arguments)
    {
        using var child = Start(file, arguments);
        var stdout = child.StandardOutput.ReadToEndAsync();
        var stderr = child.StandardError.ReadToEndAsync();
        try { await child.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(5)); }
        catch { if (!child.HasExited) child.Kill(true); throw; }
        return (child.ExitCode, await stdout + await stderr);
    }
    public static async Task<string> Tool(string name, params string[] arguments)
    {
        var result = await Run(Dotnet, new[]{"tool", "run", name, "--"}.Concat(arguments).ToArray());
        Check.That(result.Code == 0, name + " exits successfully");
        return result.Text;
    }
    public static void Save(string file, object value)
    {
        Directory.CreateDirectory("artifacts");
        File.WriteAllText(Path.Combine("artifacts",file), JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}
