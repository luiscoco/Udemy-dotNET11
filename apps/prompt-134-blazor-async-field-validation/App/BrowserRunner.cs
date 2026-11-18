using System.Diagnostics;
public static class BrowserRunner
{
    public static async Task RunAsync(string address, string lesson, string mode)
    {
        var start = new ProcessStartInfo("node")
        {
            UseShellExecute = false, RedirectStandardOutput = true,
            RedirectStandardError = true, CreateNoWindow = true
        };
        foreach (var argument in new[] { "browser-demo.cjs", address, lesson, mode })
            start.ArgumentList.Add(argument);
        using var child = Process.Start(start)!;
        var output = child.StandardOutput.ReadToEndAsync();
        var error = child.StandardError.ReadToEndAsync();
        await child.WaitForExitAsync();
        Console.Write(await output);
        if (child.ExitCode != 0)
            throw new InvalidOperationException(await error);
    }
}
