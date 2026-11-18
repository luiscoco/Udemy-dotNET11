using System.Diagnostics;
public static class BrowserRunner
{
    public static async Task RunAsync(string address,string scenario)
    {
        var start=new ProcessStartInfo("node") {UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
        start.ArgumentList.Add("browser-demo.cjs"); start.ArgumentList.Add(address); start.ArgumentList.Add(scenario);
        using var process=Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); Console.Write(await output); string diagnostics=await errors;
        if(process.ExitCode!=0) throw new InvalidOperationException("Browser verification failed: "+diagnostics);
        Check.That(process.ExitCode==0,"the real browser workflow completed and saved its screenshot evidence");
    }
}
