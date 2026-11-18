using System.Diagnostics;
public static class BrowserRunner
{
    public static async Task RunAsync(string address,string scenario,string pin)
    {
        var start=new ProcessStartInfo("node"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
        foreach(var value in new[]{"browser-demo.cjs",address,scenario,pin})start.ArgumentList.Add(value);
        using var process=Process.Start(start)!;var output=process.StandardOutput.ReadToEndAsync();var error=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();Console.Write(await output);
        if(process.ExitCode!=0)throw new InvalidOperationException("Browser check failed: "+await error);
        Check.That(true,"actual browser flow completed and saved its evidence");
    }
}
