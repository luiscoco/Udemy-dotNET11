using System.Text.Json;
Directory.CreateDirectory("artifacts");
string emptyRoot=Path.GetFullPath("artifacts/empty-runtime");
Directory.CreateDirectory(emptyRoot);
var observations=new List<object>();
foreach(var model in new[]{(Name:"framework",SelfContained:false,Single:false),(Name:"self-contained",SelfContained:true,Single:false),(Name:"single-file",SelfContained:true,Single:true)})
{
    string folder=Path.GetFullPath("artifacts/published/"+model.Name);
    await DeploymentTools.Dotnet("publish","Receipt","-c","Release","-r","win-x64","--self-contained",model.SelfContained.ToString().ToLowerInvariant(),"-p:PublishSingleFile="+model.Single.ToString().ToLowerInvariant(),"-p:IncludeNativeLibrariesForSelfExtract=true","-o",folder);
    bool missingRuntime=args.FirstOrDefault()=="problem"||model.SelfContained;
    string runtime=missingRuntime?emptyRoot:Path.GetDirectoryName(Harness.Dotnet)!;
    using var child=Harness.Start(Path.Combine(folder,"Receipt.exe"),["3"],new Dictionary<string,string>{{"DOTNET_ROOT_X64",runtime},{"DOTNET_ROOT",runtime},{"DOTNET_MULTILEVEL_LOOKUP","0"}});
    var stdout=child.StandardOutput.ReadToEndAsync(); var stderr=child.StandardError.ReadToEndAsync();
    await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
    string output=await stdout,error=await stderr;
    long bytes=Directory.EnumerateFiles(folder,"*",SearchOption.AllDirectories).Where(file=>!file.EndsWith(".pdb")).Sum(file=>new FileInfo(file).Length);
    int files=Directory.EnumerateFiles(folder).Count(file=>!file.EndsWith(".pdb"));
    Console.WriteLine($"{model.Name} files={files}, bytes={bytes}, exit={child.ExitCode}");
    if(child.ExitCode!=0) Console.WriteLine(error);
    observations.Add(new {model=model.Name,model.SelfContained,model.Single,files,bytes,exitCode=child.ExitCode,output,error});
    Harness.Save("deployment-models.json",observations);
    Check.That(child.ExitCode==0,"RUNTIME_DEPENDENCY_REGRESSION selected deployment can locate its runtime");
    using var json=JsonDocument.Parse(output);
    Check.That(json.RootElement.GetProperty("Total").GetDecimal()==75m,"identical published receipt result");
    if(model.Single) Check.That(files==1,"single-file deployment has one application payload file");
}
