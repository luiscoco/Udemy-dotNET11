using System.Reflection;
using System.Text.Json;
if(args.FirstOrDefault()=="worker")
{
    string prefix=args[1];
    long allocationBefore=GC.GetTotalAllocatedBytes(true);
    int gen2Before=GC.CollectionCount(2);
    PayloadCache.Load();
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    long retainedBytes=GC.GetTotalMemory(true);
    File.WriteAllText(prefix+".ready","ready");
    while(!File.Exists(prefix+".release")) await Task.Delay(50);
    PayloadCache.Release();
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    long releasedBytes=GC.GetTotalMemory(true);
    File.WriteAllText(prefix+".released","released");
    while(!File.Exists(prefix+".stop")) await Task.Delay(50);
    File.WriteAllText(prefix+".json",JsonSerializer.Serialize(new { allocatedBytes=GC.GetTotalAllocatedBytes(true)-allocationBefore, retainedBytes, releasedBytes, retainedObjects=PayloadCache.Count, gen2Collections=GC.CollectionCount(2)-gen2Before }));
    return;
}
Directory.CreateDirectory("artifacts");
string capturePrefix=Path.GetFullPath("artifacts/heap-"+Guid.NewGuid().ToString("N"));
using var child=Harness.Start(Harness.Dotnet,[Assembly.GetExecutingAssembly().Location,"worker",capturePrefix]);
try
{
    for(int i=0;!File.Exists(capturePrefix+".ready")&&i<200;i++) await Task.Delay(50);
    Check.That(File.Exists(capturePrefix+".ready"),"owned heap worker ready");
    await Harness.Tool("dotnet-gcdump","collect","-p",child.Id.ToString(),"-o",capturePrefix+"-retained.gcdump");
    string before=await Harness.Tool("dotnet-gcdump","report",capturePrefix+"-retained.gcdump");
    File.WriteAllText(capturePrefix+"-retained.txt",before);
    Console.WriteLine(string.Join(Environment.NewLine,before.Split((char)10).Where(line=>line.Contains("CoursePayload")||line.Contains("System.Byte[]")||line.Contains("Heap")||line.Contains("Total"))));
    Check.That(before.Contains("CoursePayload"),"heap report identifies retained CoursePayload objects");
    File.WriteAllText(capturePrefix+".release","release");
    for(int i=0;!File.Exists(capturePrefix+".released")&&i<200;i++) await Task.Delay(50);
    Check.That(File.Exists(capturePrefix+".released"),"strong cache references released");
    await Harness.Tool("dotnet-gcdump","collect","-p",child.Id.ToString(),"-o",capturePrefix+"-released.gcdump");
    string after=await Harness.Tool("dotnet-gcdump","report",capturePrefix+"-released.gcdump");
    File.WriteAllText(capturePrefix+"-released.txt",after);
    Check.That(!after.Split((char)10).Any(line=>line.Contains(" CoursePayload  [")),"released snapshot contains no live CoursePayload instances");
    File.WriteAllText(capturePrefix+".stop","stop");
    await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
    using var report=JsonDocument.Parse(File.ReadAllText(capturePrefix+".json"));
    Harness.Save("heap-summary.json",report.RootElement);
    Console.WriteLine(report.RootElement);
    long beforeBytes=report.RootElement.GetProperty("retainedBytes").GetInt64();
    long afterBytes=report.RootElement.GetProperty("releasedBytes").GetInt64();
    long selected=args.FirstOrDefault()=="problem"?beforeBytes:afterBytes;
    Check.That(selected<beforeBytes/2,"RETENTION_REGRESSION released live heap falls below half of retained heap");
}
finally { if(!child.HasExited) child.Kill(true); }
