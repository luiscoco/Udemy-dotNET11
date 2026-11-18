using System.Diagnostics;
using System.Reflection;
public static class ChildRunner
{
    public static ProcessStartInfo StartInfo(params string[] arguments)
    {
        string host = Environment.ProcessPath!;
        var info = new ProcessStartInfo(host) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        info.ArgumentList.Add("child");
        foreach (string argument in arguments) info.ArgumentList.Add(argument);
        return info;
    }
    public static Task<ProcessTextOutput> CaptureAsync(string[] arguments, CancellationToken token = default) =>
        Process.RunAndCaptureTextAsync(StartInfo(arguments), token);
}
