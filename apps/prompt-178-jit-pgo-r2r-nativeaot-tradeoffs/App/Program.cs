using System.Diagnostics;
using System.Text.Json;
NativePublish.Configure();
Directory.CreateDirectory("artifacts");
foreach(var item in new[]{(Name:"managed",Aot:false,R2R:false),(Name:"r2r",Aot:false,R2R:true),(Name:"aot",Aot:true,R2R:false)})
{
    var result=await Harness.Run(Harness.Dotnet,NativePublish.Arguments("Workload","artifacts/published/"+item.Name,item.Aot,item.R2R));
    File.WriteAllText("artifacts/publish-"+item.Name+".txt",result.Text);
    Check.That(result.Code==0,"published actual "+item.Name+" executable");
}
var profiles=new[]{
    (Name:"JIT",Folder:"managed",Tier:"0",Pgo:"0"),
    (Name:"Tiered",Folder:"managed",Tier:"1",Pgo:"0"),
    (Name:"DynamicPGO",Folder:"managed",Tier:"1",Pgo:"1"),
    (Name:"ReadyToRun",Folder:"r2r",Tier:"1",Pgo:"1"),
    (Name:"NativeAOT",Folder:"aot",Tier:"0",Pgo:"0")};
var observations=new List<object>();
var capabilities=new Dictionary<string,bool>();
foreach(var profile in profiles)
for(int repeat=0;repeat<3;repeat++)
{
    string folder=Path.GetFullPath("artifacts/published/"+profile.Folder);
    var clock=Stopwatch.StartNew();
    using var child=Harness.Start(Path.Combine(folder,"Workload.exe"),[],new Dictionary<string,string>{{"DOTNET_TieredCompilation",profile.Tier},{"DOTNET_TieredPGO",profile.Pgo}});
    var stdout=child.StandardOutput.ReadToEndAsync(); var stderr=child.StandardError.ReadToEndAsync();
    await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30)); clock.Stop();
    Check.That(child.ExitCode==0,"profile completed "+profile.Name+await stderr);
    using var json=JsonDocument.Parse(await stdout);
    var report=json.RootElement;
    Check.That(report.GetProperty("Checksum").GetInt64()==1020000000,"all deployment outputs preserve the checksum");
    bool dynamic=report.GetProperty("DynamicCodeSupported").GetBoolean();
    capabilities[profile.Name]=dynamic;
    Check.That(dynamic==(profile.Folder!="aot"),"actual dynamic-code capability matches deployment");
    long bytes=Directory.EnumerateFiles(folder,"*",SearchOption.AllDirectories).Where(f=>!f.EndsWith(".pdb")).Sum(f=>new FileInfo(f).Length);
    observations.Add(new { profile=profile.Name,repeat,profile.Tier,profile.Pgo,bytes,processMs=clock.Elapsed.TotalMilliseconds,report=report.Clone() });
    Console.WriteLine($"{profile.Name} sample={repeat+1}, process={clock.Elapsed.TotalMilliseconds:F1} ms, kernel={report.GetProperty("Milliseconds").GetDouble():F2} ms, bytes={bytes}, dynamic={dynamic}");
}
Harness.Save("deployment-observations.json",observations);
string selected=args.FirstOrDefault()=="problem"?"JIT":"NativeAOT";
Check.That(!capabilities[selected],"DEPLOYMENT_REGRESSION selected release must have the verified Native AOT capability");
