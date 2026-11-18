using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
var boundary=new ApprovalGate(); bool unapprovedDenied=false;
try { boundary.Apply(args.Contains("problem")); } catch(UnauthorizedAccessException) { unapprovedDenied=true; }
Console.WriteLine($"APPROVAL missingGrantDenied={unapprovedDenied} effects={boundary.Effects}");
Check.That(unapprovedDenied&&boundary.Effects==0,"missing human grant prevents mutation","AGENT unapproved side effect executed");
using var client=AgentRuntime.Client(); var gate=new ApprovalGate();
var publish=AIFunctionFactory.Create(()=>gate.Apply(),"PublishDraft","Publish the already displayed lesson draft to the local teaching outbox.");
AIAgent agent=AgentRuntime.Create(client,"ApprovalTutor","Call PublishDraft once when asked to publish. Never retry a denied call.",
    [new ApprovalRequiredAIFunction(publish)]);
async Task<(AgentSession Session,ToolApprovalRequestContent Request)> Propose()
{
    var session=await agent.CreateSessionAsync();
    var response=await agent.RunAsync("Please use PublishDraft to publish the displayed lesson draft.",session,
        cancellationToken:new CancellationTokenSource(TimeSpan.FromSeconds(90)).Token);
    var pending=response.Messages.SelectMany(m=>m.Contents).OfType<ToolApprovalRequestContent>().ToArray();
    Check.That(pending.Length==1&&gate.Effects==0,"framework returns an approval request before executing the tool");
    return (session,pending.Single());
}
var denied=await Propose();
await agent.RunAsync(new ChatMessage(ChatRole.User,[denied.Request.CreateResponse(false,"Human fixture declined. Do not retry.")]),denied.Session);
Check.That(gate.Effects==0,"explicit decline keeps the outbox unchanged");
var allowed=await Propose();
string displayedHash=gate.Current.Hash;
Console.WriteLine("APPROVAL displayed actor=alice target=local-course-outbox hash="+displayedHash[..12]);
// The automated teaching harness supplies the explicit human decision here.
gate.Approve("alice",displayedHash);
await agent.RunAsync(new ChatMessage(ChatRole.User,[allowed.Request.CreateResponse(true)]),allowed.Session);
Check.That(gate.Effects==1,"explicit approval executes exactly one local side effect");
bool replayDenied=false; try { gate.Apply(); } catch(UnauthorizedAccessException) { replayDenied=true; }
var changed=new ApprovalGate(); changed.Approve("alice",changed.Current.Hash);
changed.Current=changed.Current with { Body="Changed after review" };
bool tamperDenied=false; try { changed.Apply(); } catch(UnauthorizedAccessException) { tamperDenied=true; }
var expired=new ApprovalGate(); expired.Approve("alice",expired.Current.Hash);
expired.Current=expired.Current with { Expires=DateTimeOffset.UtcNow.AddSeconds(-1) };
bool expiryDenied=false; try { expired.Apply(); } catch(UnauthorizedAccessException) { expiryDenied=true; }
Check.That(replayDenied&&tamperDenied&&expiryDenied,"replay, changed payload and expired approval fail closed");
AgentRuntime.Save(new { passed=true, pendingBeforeEffect=true, declinedEffects=0, approvedEffects=gate.Effects,replayDenied,tamperDenied,expiryDenied,decisionSource="explicit automated human-decision fixture" });
