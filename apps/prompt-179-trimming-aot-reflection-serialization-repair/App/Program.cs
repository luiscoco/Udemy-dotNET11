NativePublish.Configure();
Directory.CreateDirectory("artifacts");
if(args.FirstOrDefault()=="problem")
{
    string[] publish=NativePublish.Arguments("Unsafe","artifacts/published/unsafe",true)
        .Concat(new[]{"-p:ILLinkTreatWarningsAsErrors=true","-p:IlcTreatWarningsAsErrors=true","-p:TrimmerSingleWarn=false"}).ToArray();
    var result=await Harness.Run(Harness.Dotnet,publish);
    File.WriteAllText("artifacts/unsafe-publish.txt",result.Text);
    foreach(string line in result.Text.Split((char)10).Where(line=>line.Contains("IL2026")||line.Contains("IL3050"))) Console.WriteLine(line);
    Check.That(result.Code!=0&&result.Text.Contains("IL2026")&&result.Text.Contains("IL3050"),"actual compatibility diagnostics reject the unsafe implementation");
    Check.That(result.Code==0,"AOT_COMPATIBILITY_REGRESSION unsafe release fails the compatibility requirement");
}
var safe=await Harness.Run(Harness.Dotnet,NativePublish.Arguments("Safe","artifacts/published/safe",true)
    .Concat(new[]{"-p:ILLinkTreatWarningsAsErrors=true","-p:IlcTreatWarningsAsErrors=true"}).ToArray());
File.WriteAllText("artifacts/safe-publish.txt",safe.Text);
Check.That(safe.Code==0,"repaired native publish passes compatibility diagnostics");
var run=await Harness.Run(Path.GetFullPath("artifacts/published/safe/Safe.exe"),"course");
Console.WriteLine(run.Text);
Check.That(run.Code==0&&run.Text.Contains("generated metadata"),"actual native application round-trips its course DTO");
var unknown=await Harness.Run(Path.GetFullPath("artifacts/published/safe/Safe.exe"),"unknown");
Check.That(unknown.Code==2,"closed registry rejects an unknown type name");
Harness.Save("compatibility-summary.json",new { published=true,executed=true,roundTrip=true,unknownTypeRejected=true });
