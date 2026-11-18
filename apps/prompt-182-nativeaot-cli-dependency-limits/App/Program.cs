NativePublish.Configure();
Directory.CreateDirectory("artifacts");
if(args.FirstOrDefault()=="problem")
{
    var command=NativePublish.Arguments("Unsupported","artifacts/published/unsupported",true)
        .Concat(new[]{"-p:ILLinkTreatWarningsAsErrors=true","-p:IlcTreatWarningsAsErrors=true"}).ToArray();
    var result=await Harness.Run(Harness.Dotnet,command);
    File.WriteAllText("artifacts/dependency-diagnostics.txt",result.Text);
    Console.WriteLine(result.Text);
    Check.That(result.Code==0,"NATIVE_DEPENDENCY_REGRESSION dependency must pass native compatibility checks");
}
await DeploymentTools.Dotnet(NativePublish.Arguments("Receipt","artifacts/published/native",true)
    .Concat(new[]{"-p:ILLinkTreatWarningsAsErrors=true","-p:IlcTreatWarningsAsErrors=true"}).ToArray());
string executable=Path.GetFullPath("artifacts/published/native/Receipt.exe");
var valid=await Harness.Run(executable,"3");
Console.WriteLine(valid.Text);
Check.That(valid.Code==0&&valid.Text.Contains("DynamicCodeSupported=False"),"actual native CLI executes without dynamic-code support");
Check.That(valid.Text.Contains("75"),"native receipt preserves its total");
var invalid=await Harness.Run(executable,"-1");
Check.That(invalid.Code==2,"native CLI rejects invalid quantity");
Harness.Save("native-cli.json",new {published=true,executed=true,dynamicCodeSupported=false,validExit=valid.Code,invalidExit=invalid.Code});
