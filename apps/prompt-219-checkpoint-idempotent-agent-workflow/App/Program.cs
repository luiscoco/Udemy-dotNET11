using System.Reflection;
if(args[0]=="worker")
{
    string folder=args[1]; bool crash=args.Contains("crash"),broken=args.Contains("broken");
    string checkpoint=Path.Combine(folder,"checkpoint.json");
    if(!File.Exists(checkpoint))
    {
        using var client=AgentRuntime.Client();
        var agent=AgentRuntime.Create(client,"DraftWriter","Write one short course completion message. No tools or external actions.");
        string body=await AgentRuntime.Ask(agent,"The student completed the synthetic NET11 lab.");
        await DurableFlow.Save(folder,new(1,"alice","job-7",body,false));
    }
    var state=await DurableFlow.Load(folder);
    if(!state.Sent)
    {
        string key=broken?Guid.NewGuid().ToString("N"):state.JobId+":confirmation:v1";
        await DurableFlow.Effect(folder,key,state.Body);
        Console.WriteLine("CHECKPOINT effect committed; app checkpoint still pending");
        if(crash) Environment.Exit(42);
        await DurableFlow.Save(folder,state with { Version=2,Sent=true });
    }
    Console.WriteLine("CHECKPOINT complete"); return;
}
string owned=Path.GetFullPath(Path.Combine("artifacts","workflow-"+Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(owned); string dll=Assembly.GetExecutingAssembly().Location;
string mode=args.Contains("problem")?"broken":"safe";
var crashRun=await Harness.Run(Harness.Dotnet,dll,"worker",owned,"crash",mode); Console.WriteLine(crashRun.Text);
Check.That(crashRun.Code==42,"actual worker process exits after the effect commits");
var resumed=await Harness.Run(Harness.Dotnet,dll,"worker",owned,"resume",mode); Console.WriteLine(resumed.Text);
Check.That(resumed.Code==0,"fresh process resumes the saved proposal");
long effects=await DurableFlow.Count(owned); var final=await DurableFlow.Load(owned);
Console.WriteLine($"CHECKPOINT receipts={effects} completed={final.Sent} version={final.Version}");
Check.That(effects==1,"stable operation identity prevents duplicate external effects","AGENT workflow duplicated external effect");
Check.That(final.Sent&&final.Version==2,"completion checkpoint advances only after effect acknowledgment");
var replay=await Harness.Run(Harness.Dotnet,dll,"worker",owned,"resume",mode);
Check.That(replay.Code==0&&await DurableFlow.Count(owned)==1,"replaying the completed workflow preserves one receipt");
bool payloadRejected=false;
try { await DurableFlow.Effect(owned,"job-7:confirmation:v1","Different body"); }
catch(InvalidOperationException) { payloadRejected=true; }
Check.That(payloadRejected,"reused operation identity rejects a changed payload");
AgentRuntime.Save(new { passed=true,crashExit=crashRun.Code,resumeExit=resumed.Code,receipts=effects,completed=final.Sent,version=final.Version,payloadRejected,actualProcessRestart=true });
