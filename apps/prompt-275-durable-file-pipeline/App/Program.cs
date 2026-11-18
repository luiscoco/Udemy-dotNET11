using System.Reflection;

if (args.Length > 0 && args[0] == "worker")
{
    Environment.ExitCode = Jobs.Worker(args[1], args[2]);
    return;
}

bool broken = args.Contains("problem");
string directory = Path.GetFullPath(Path.Combine("artifacts", "pipeline-" + Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(directory);
File.WriteAllText(Path.Combine(directory, "input.txt"), "Durable teaching payload: invoice 42");
var store = new Store(Path.Combine(directory, "jobs.db"), true);
store.Execute("CREATE TABLE Jobs(Id INTEGER PRIMARY KEY,State TEXT,Attempts INTEGER,Lease INTEGER,Cancelled INTEGER,Retries INTEGER); INSERT INTO Jobs VALUES(1,'Pending',0,0,0,0); CREATE TABLE Effects(JobId INTEGER PRIMARY KEY,Hash TEXT)");
async Task<int> Execute(string mode)
{
    var result = await Harness.Run(Harness.Dotnet, Assembly.GetExecutingAssembly().Location, "worker", store.FileName, mode);
    Console.WriteLine("JOBS process=" + mode + " actualExit=" + result.Code);
    return result.Code;
}

Check.That(await Execute("crash-after-claim") == 17, "first worker really exits after durable claim");
await Task.Delay(700);
int recovered = broken ? 0 : Jobs.Recover(store);
Console.WriteLine("JOBS expired claims recovered=" + recovered);
if (broken)
{
    await Execute("finish");
    Check.That(store.Scalar("SELECT COUNT(*) FROM Jobs WHERE State='Completed'") == 1, "restarted worker completes abandoned job", "CAPSTONE durable job recovery failed");
}

Check.That(recovered == 1, "restart recovery requeues the actual expired lease");
Check.That(await Execute("cancel") == 0 && store.Scalar("SELECT Cancelled FROM Jobs") == 1, "real cancelled copy retains a pending durable job");
Check.That(await Execute("transient") == 0 && store.Scalar("SELECT Retries FROM Jobs") == 1, "transient failure records a bounded retry");
Check.That(await Execute("crash-after-output") == 18 && File.Exists(Path.Combine(directory, "output.txt")), "second worker really exits between output and acknowledgement");
await Task.Delay(700);
Check.That(Jobs.Recover(store) == 1, "second expired lease is recovered");
await Execute("finish");
await Execute("finish");
long effects = store.Scalar("SELECT COUNT(*) FROM Effects"), attempts = store.Scalar("SELECT Attempts FROM Jobs");
Check.That(store.Scalar("SELECT COUNT(*) FROM Jobs WHERE State='Completed'") == 1 && effects == 1 && attempts == 5, "recovery verifies existing output and commits one logical effect");
Harness.Save("capstone-evidence.json", new { passed = true, actualSqlite = true, actualChildProcesses = true, crashCodes = new[] { 17, 18 }, cancellation = 1, retries = 1, attempts, effects, outputFiles = Directory.GetFiles(directory, "output.txt").Length, scope = "single-host durable queue; bounded retries; idempotent output hash; not distributed exactly-once" });
