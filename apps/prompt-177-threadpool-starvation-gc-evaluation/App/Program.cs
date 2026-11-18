using System.Reflection;
using System.Text.Json;
if(args.FirstOrDefault()=="worker")
{
    object report=await PoolExperiment.Run(args[1]=="async");
    File.WriteAllText(args[2],JsonSerializer.Serialize(report));
    return;
}
Directory.CreateDirectory("artifacts");
var results=new List<JsonElement>();
foreach(bool server in new[]{false,true})
foreach(string mode in new[]{"blocking","async"})
{
    string report=Path.GetFullPath($"artifacts/{mode}-{server}.json");
    using var child=Harness.Start(Harness.Dotnet,[Assembly.GetExecutingAssembly().Location,"worker",mode,report],
        new Dictionary<string,string>{{"DOTNET_gcServer",server?"1":"0"},{"DOTNET_GCHeapCount","2"}});
    var stdout=child.StandardOutput.ReadToEndAsync();
    var stderr=child.StandardError.ReadToEndAsync();
    await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
    Check.That(child.ExitCode==0,"owned worker completed "+mode+" "+await stderr);
    await stdout;
    using var json=JsonDocument.Parse(File.ReadAllText(report));
    var value=json.RootElement.Clone(); results.Add(value);
    Check.That(value.GetProperty("serverGc").GetBoolean()==server,"requested GC mode is active");
    bool completed=value.GetProperty("completedBeforeRelease").GetBoolean();
    Check.That(completed==(mode=="async"),"probe progress matches the wait implementation");
    Console.WriteLine(value);
}
Harness.Save("pool-gc-summary.json",results);
bool problem=args.FirstOrDefault()=="problem";
var selected=results.First(r=>r.GetProperty("asynchronous").GetBoolean()==!problem);
Check.That(selected.GetProperty("completedBeforeRelease").GetBoolean(),"QUEUE_REGRESSION unrelated queued work progresses before release");
