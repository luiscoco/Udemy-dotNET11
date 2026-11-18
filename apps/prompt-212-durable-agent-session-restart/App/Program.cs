using Microsoft.Agents.AI;
using System.Reflection;
using System.Text.Json;
if (args[0] is "write" or "resume")
{
    string stage=args[0], folder=args[1]; bool broken=args.Contains("broken");
    using var client=AgentRuntime.Client();
    AIAgent agent=AgentRuntime.Create(client,"ProjectTutor","Remember the user's project codename. Answer briefly.");
    string file=Path.Combine(folder,"session.json");
    if (stage == "write")
    {
        var session=await agent.CreateSessionAsync();
        await AgentRuntime.Ask(agent,"My project codename is Atlas-A7. Remember it.",session);
        var state=await agent.SerializeSessionAsync(session);
        await SessionFile.Save(file,new(1,"alice",AgentRuntime.Model,state));
        Console.WriteLine("SESSION durable save complete"); return;
    }
    var saved=await SessionFile.Load(file,"alice");
    AgentSession restored=broken ? await agent.CreateSessionAsync() : await agent.DeserializeSessionAsync(saved.State);
    await AgentRuntime.Ask(agent,"What project codename did I give you?",restored);
    bool history=client.InputText.Contains("Atlas-A7");
    Console.WriteLine($"SESSION restoredHistory={history}");
    Check.That(history,"restored model input contains the earlier user fact","AGENT restart lost conversation history");
    Harness.Save(Path.Combine(folder,"resume.json"),new { passed=true, history, replyInput=client.InputText }); return;
}
string owned=Path.GetFullPath(Path.Combine("artifacts","session-"+Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(owned); string executable=Assembly.GetExecutingAssembly().Location;
var written=await Harness.Run(Harness.Dotnet,executable,"write",owned); Console.WriteLine(written.Text);
Check.That(written.Code==0,"first actual process saves the framework session");
bool ownerRejected=false;
try { await SessionFile.Load(Path.Combine(owned,"session.json"),"bob"); } catch (UnauthorizedAccessException) { ownerRejected=true; }
Check.That(ownerRejected,"session ownership is checked before deserialization");
var resumed=await Harness.Run(Harness.Dotnet,executable,"resume",owned,args.Contains("problem")?"broken":"safe");
Console.WriteLine(resumed.Text);
Check.That(resumed.Code==0,"fresh process restores the conversation","AGENT restart lost conversation history");
AgentRuntime.Save(new { passed=true, actualProcesses=2, writeExit=written.Code, resumeExit=resumed.Code, ownerRejected });
