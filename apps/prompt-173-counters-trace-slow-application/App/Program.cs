using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
if(args.FirstOrDefault() == "worker")
{
    bool cached = args[1] == "cached";
    string prefix = args[2];
    long result = CpuWork.Compute(11), requests = 0;
    File.WriteAllText(prefix+".ready", "ready");
    while(!File.Exists(prefix+".stop"))
    {
        if(!cached) result = CpuWork.Compute(11);
        else await Task.Delay(2);
        requests++;
    }
    File.WriteAllText(prefix+".json",JsonSerializer.Serialize(new { cached, requests, computations=CpuWork.Computations, result, cpuMs=Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds }));
    return;
}
Directory.CreateDirectory("artifacts");
var observations = new Dictionary<string,JsonElement>();
foreach(string mode in new[]{"repeated","cached"})
{
    string prefix = Path.GetFullPath("artifacts/"+mode+"-"+Guid.NewGuid().ToString("N"));
    using var child = Harness.Start(Harness.Dotnet,[Assembly.GetExecutingAssembly().Location,"worker",mode,prefix]);
    try
    {
        for(int i=0;!File.Exists(prefix+".ready") && i<200;i++) await Task.Delay(50);
        Check.That(File.Exists(prefix+".ready"),"owned worker ready "+mode);
        var counters=await Harness.Tool("dotnet-counters","collect","-p",child.Id.ToString(),"--counters","System.Runtime","--format","json","--duration","00:00:04","-o",prefix+"-counters.json");
        var trace=await Harness.Tool("dotnet-trace","collect","-p",child.Id.ToString(),"--profile","dotnet-sampled-thread-time","--duration","00:00:04","-o",prefix+".nettrace");
        File.WriteAllText(prefix+"-collection-output.txt",counters+trace);
        string report=await Harness.Tool("dotnet-trace","report",prefix+".nettrace","topN","-n","8");
        File.WriteAllText(prefix+"-top.txt",report);
        Check.That(new FileInfo(prefix+".nettrace").Length>1000,"actual EventPipe trace retained");
        Check.That(File.ReadAllText(prefix+"-counters.json").Contains("dotnet."),"runtime metric samples retained");
        if(mode=="repeated") Console.WriteLine(report);
        File.WriteAllText(prefix+".stop", "stop");
        await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        using var json=JsonDocument.Parse(File.ReadAllText(prefix+".json"));
        observations[mode]=json.RootElement.Clone();
        Console.WriteLine(mode+" "+json.RootElement);
    }
    finally { if(!child.HasExited) child.Kill(true); }
}
Check.That(observations["repeated"].GetProperty("result").GetInt64()==observations["cached"].GetProperty("result").GetInt64(),"cache preserves the computed result");
Harness.Save("diagnosis-summary.json", observations);
string selected=args.FirstOrDefault()=="problem"?"repeated":"cached";
Check.That(observations[selected].GetProperty("computations").GetInt64()==1,"REPEATED_WORK_REGRESSION one computation per repeated key");
