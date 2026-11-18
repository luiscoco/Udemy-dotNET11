using System.Text.Json;
if(args.Contains("offline-worker"))
{
    string childWork=Environment.GetEnvironmentVariable("COURSE_FOUNDRY_WORKER_DIR")!;
    try{await FoundryWorker.Execute();}catch(Exception e){File.WriteAllText(Path.Combine(childWork,"worker-error.txt"),e.ToString());Environment.ExitCode=3;}return;
}
string path=Environment.GetEnvironmentVariable("COURSE_FOUNDRY_MODEL_PATH")??"";
if(args.Contains("problem"))path=Path.Combine("artifacts","deliberately-missing-model");
Console.WriteLine($"FOUNDRY modelDirectoryPresent={Directory.Exists(path)}");
Check.That(Directory.Exists(path)&&File.Exists(Path.Combine(path,"genai_config.json")),"the pre-acquired compatible model is present before offline startup","FOUNDRY required model cache is missing");
string worker=Path.GetFullPath("artifacts/offline-worker/App.exe");
Check.That(File.Exists(worker),"the self-contained worker was published for OS isolation");
string work=Path.GetFullPath(Path.Combine("artifacts","offline-run-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(work);
int exit=await OfflineSandbox.Run(worker,Path.GetFullPath(path),work);
if(exit!=0&&File.Exists(Path.Combine(work,"worker-error.txt")))Console.WriteLine(File.ReadAllText(Path.Combine(work,"worker-error.txt")));
Check.That(exit==0,"the fresh OS-isolated Foundry worker completes successfully");
using var result=JsonDocument.Parse(File.ReadAllText(Path.Combine(work,"worker-result.json")));
Console.WriteLine("FOUNDRY "+result.RootElement);
Check.That(result.RootElement.GetProperty("networkDenied").GetBoolean(),"the operating system denied external sockets during actual cached inference");
Harness.Save("ml-evidence.json",new {passed=true,actualFoundry=true,actualOfflineWorker=true,networkDenied=true,workerExit=exit,result=result.RootElement});
