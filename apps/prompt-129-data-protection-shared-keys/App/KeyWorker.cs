using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
public sealed record WorkerRequest(string Operation,string Application,string Purpose,string Value);
public sealed record WorkerResult(bool Success,string? Value,int ProcessId);
public static class KeyWorker
{
    public static WorkerResult Execute(WorkerRequest input)
    {
        using var certificate=X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(Environment.GetEnvironmentVariable("COURSE_CERT_PFX")!),null);
        using var services=KeyRepository.Build(Environment.GetEnvironmentVariable("COURSE_KEY_DIRECTORY")!,certificate,input.Application);
        var protector=services.GetRequiredService<IDataProtectionProvider>().CreateProtector(input.Purpose);
        try{return new(true,input.Operation=="protect"?protector.Protect(input.Value):protector.Unprotect(input.Value),Environment.ProcessId);}
        catch(CryptographicException){return new(false,null,Environment.ProcessId);}
    }
    public static async Task<WorkerResult> Run(string directory,X509Certificate2 certificate,WorkerRequest request)
    {
        var start=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};
        if(string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath),"dotnet",StringComparison.OrdinalIgnoreCase))start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        start.ArgumentList.Add("worker");start.Environment["COURSE_KEY_DIRECTORY"]=directory;
        start.Environment["COURSE_CERT_PFX"]=Convert.ToBase64String(certificate.Export(X509ContentType.Pfx));
        using var child=Process.Start(start)!;await child.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));child.StandardInput.Close();
        var output=child.StandardOutput.ReadToEndAsync();var errors=child.StandardError.ReadToEndAsync();await child.WaitForExitAsync();
        if(child.ExitCode!=0)throw new InvalidOperationException("Owned key worker failed: "+await errors);
        return JsonSerializer.Deserialize<WorkerResult>(await output)!;
    }
}
