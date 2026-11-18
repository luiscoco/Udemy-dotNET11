using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.Sqlite;
if(args.FirstOrDefault()=="worker")
{await SchedulerWorker.Run(args[1],args[2]=="memory",args[3]=="seed");return;}
var memory=args.FirstOrDefault()=="memory";
var folder=Path.GetFullPath(Path.Combine("artifacts","restart-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
Process Start(string stage)
{
    var start=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
    if(Path.GetFileNameWithoutExtension(Environment.ProcessPath!).Equals("dotnet",StringComparison.OrdinalIgnoreCase))start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    foreach(var value in new[]{"worker",folder,memory?"memory":"persistent",stage})start.ArgumentList.Add(value);
    var process=Process.Start(start)!;process.OutputDataReceived+=(_,e)=>{if(e.Data is not null)Console.WriteLine(e.Data);};
    process.ErrorDataReceived+=(_,e)=>{if(e.Data is not null)Console.WriteLine(e.Data);};process.BeginOutputReadLine();process.BeginErrorReadLine();return process;
}
using var original=Start("seed");
try
{
    var until=DateTime.UtcNow.AddSeconds(25);
    while(!File.Exists(Path.Combine(folder,"started.txt")) && !original.HasExited && DateTime.UtcNow<until)await Task.Delay(25);
    Check.That(File.Exists(Path.Combine(folder,"started.txt")),"original worker entered the scheduled job");
    var firstPid=original.Id;original.Kill(entireProcessTree:true);await original.WaitForExitAsync();
    Console.WriteLine($"service: deliberately stopped owned worker PID {firstPid} after job entry");
    using var restarted=Start("resume");
    await restarted.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(25));
    Check.That(restarted.Id!=firstPid && restarted.ExitCode==0,"a different worker process reopened the scheduler");
    var completed=File.Exists(Path.Combine(folder,"completed.txt"));
    if(memory)Check.That(completed,"incorrect expectation: memory jobs survive process restart");
    Check.That(completed && File.ReadAllText(Path.Combine(folder,"completed.txt"))=="Recovering=True",
        "persistent job store recovers the interrupted RequestRecovery job");
    using var db=new SqliteConnection($"Data Source={Path.Combine(folder,"effects.db")};Pooling=False");db.Open();
    using var count=db.CreateCommand();count.CommandText="SELECT COUNT(*) FROM Effects";
    Check.That(Convert.ToInt32(count.ExecuteScalar())==1,"recovered job leaves one idempotent business effect");
}
finally{if(!original.HasExited){original.Kill(entireProcessTree:true);await original.WaitForExitAsync();}}
