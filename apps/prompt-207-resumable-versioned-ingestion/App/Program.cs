using System.Reflection;
if (args.FirstOrDefault()=="worker")
{
    await IngestionWorker.Run(args[1],args[2]=="broken",args[3]=="crash");return;
}
bool broken=args.Contains("problem");
await using var store=await VectorStore.Open(207);
await store.CreateCollection();
await store.Execute("CREATE TABLE journal(seq int PRIMARY KEY,id text,body text,version int,deleted bool); CREATE TABLE checkpoint(singleton int PRIMARY KEY,last_seq int); INSERT INTO checkpoint VALUES(1,0); INSERT INTO journal VALUES(1,'keys','Rotate keys every 30 days.',1,false),(2,'backup','Restore the database backup.',1,false)");
string executable=Assembly.GetExecutingAssembly().Location;
async Task<int> Worker(bool crash)
{
    var result=await Harness.Run(Harness.Dotnet,executable,"worker",store.Schema,broken?"broken":"safe",crash?"crash":"continue");
    Console.WriteLine(result.Text);return result.Code;
}
Check.That(await Worker(false)==0,"initial ingestion worker completes in a real child process");
await store.Execute("INSERT INTO journal VALUES(3,'keys','Rotate keys every 14 days.',2,false),(4,'backup','',2,true),(5,'cache','Bypass caching sensitive data.',1,false)");
int crashExit=await Worker(true);
Check.That(crashExit==42,"controlled worker process terminates before its selected commit boundary");
int resumeExit=await Worker(false);
var keys=await store.Retrieve("keys");
bool deleted=await store.Retrieve("backup") is null;
int checkpoint=await store.Scalar<int>("SELECT last_seq FROM checkpoint WHERE singleton=1");
Console.WriteLine($"INGEST checkpoint={checkpoint} keyVersion={keys?.Version} deletionPropagated={deleted} resumeExit={resumeExit}");
Check.That(keys?.Version==2,"resumed processing preserves the latest durable document version","RAG checkpoint advanced beyond durable vectors");
Check.That(deleted&&checkpoint==5&&resumeExit==0,"deletion and checkpoint resume complete after process restart");
Check.That(await Worker(false)==0&&await store.Scalar<long>("SELECT count(*) FROM items")==2,"replaying from the completed checkpoint is idempotent");
AiRuntime.Evidence("rag-evidence.json",new { passed=true, crashExit,resumeExit,checkpoint,version=keys.Version,deleted,realProcessRestart=true });
