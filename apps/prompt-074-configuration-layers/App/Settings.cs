using System.Text.Json;
using Microsoft.Extensions.Configuration;
public sealed class DisplaySettings
{
    public string Theme { get; set; } = "";
    public string Note { get; set; } = "";
}
public static class Settings
{
    public static string PathName => Path.GetFullPath("artifacts/settings.json");
    public static void Write(string note)
    {
        Directory.CreateDirectory("artifacts");
        File.WriteAllText(PathName, JsonSerializer.Serialize(new { Demo = new { Theme = "json", Note = note } }));
    }
    public static ConfigurationRoot Build(int layers, bool wrongOrder = false)
    {
        var builder = new ConfigurationBuilder();
        string[] commandLine = ["--Demo:Theme=cli"];
        if (wrongOrder) builder.AddCommandLine(commandLine);
        builder.AddJsonFile(PathName, optional: false, reloadOnChange: true);
        if (layers >= 2) builder.AddEnvironmentVariables("LCE74_");
        if (layers >= 3 && !wrongOrder) builder.AddCommandLine(commandLine);
        return (ConfigurationRoot)builder.Build();
    }
}
