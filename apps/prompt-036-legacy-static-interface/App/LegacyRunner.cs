using System.Diagnostics;
public static class LegacyRunner
{
    public static async Task<int> Run(string language)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This legacy demonstration requires Windows .NET Framework 4");
        string root = Environment.GetEnvironmentVariable("DOTNET_ROOT")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
        string sdk = Directory.GetDirectories(Path.Combine(root, "sdk"), "11.0.*")
            .OrderDescending().First();
        string compiler = Path.Combine(sdk, "Roslyn", "bincore", "csc.dll");
        string framework = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "Microsoft.NET", "Framework64", "v4.0.30319");
        string output = Path.GetFullPath("artifacts/legacy-helper.exe");
        Directory.CreateDirectory("artifacts");
        var compile = new ProcessStartInfo(Path.Combine(root, "dotnet.exe"));
        string[] arguments = [compiler, "/nologo", "/preferreduilang:en-US", "/target:exe", "/nostdlib+",
            "/langversion:" + language, "/out:" + output,
            "/reference:" + Path.Combine(framework, "mscorlib.dll"),
            Path.GetFullPath("Legacy/LegacyHelper.cs")];
        foreach (string argument in arguments) compile.ArgumentList.Add(argument);
        Console.WriteLine($"Compile C# {language} against the CLR 4 core library");
        int exit = await Execute(compile);
        if (exit != 0) return exit;
        return await Execute(new ProcessStartInfo(output));
    }

    private static async Task<int> Execute(ProcessStartInfo start)
    {
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Process did not start");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Console.Write(await output);
        Console.Write(await error);
        return process.ExitCode;
    }
}
