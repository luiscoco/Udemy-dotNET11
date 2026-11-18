using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
var rollout=new RolloutPolicy();var feedback=new FeedbackStore();var tokens=new LocalTokens();var builder=WebFixture.Builder();tokens.Register(builder.Services);
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();
app.MapGet("/version",(ClaimsPrincipal principal)=>Results.Text(rollout.Select(principal.FindFirstValue("sub")!))).RequireAuthorization("mcp.access");
app.MapPost("/feedback/{requestId}",(string requestId,ClaimsPrincipal principal)=>
    feedback.Add(requestId,principal.FindFirstValue("sub")!,true)?Results.Ok():Results.NotFound()).RequireAuthorization("mcp.access");
await using var server=await WebFixture.Start(app);using var alice=server.Client(tokens.Issue());using var bob=server.Client(tokens.Issue(subject:"bob"));
string first=await alice.GetStringAsync("/version"),second=await alice.GetStringAsync("/version");
Check.That(first==second,"validated identity keeps a stable version bucket across actual requests");
feedback.RecordRequest("r1","alice",first);
var denied=await bob.PostAsync("/feedback/r1",null);var allowed=await alice.PostAsync("/feedback/r1",null);var duplicate=await alice.PostAsync("/feedback/r1",null);
Check.That((int)denied.StatusCode==404&&(int)allowed.StatusCode==200&&(int)duplicate.StatusCode==404&&feedback.Count==1,"feedback requires request ownership and is deduplicated by server-assigned request identity");
// Controlled labelled replay supplies version comparison; these are not live production observations.
Feedback[] replay=Enumerable.Range(0,10).Select(i=>new Feedback("b"+i,"baseline-v1",true,false))
    .Concat(Enumerable.Range(0,10).Select(i=>new Feedback("c"+i,"candidate-v2",i<3,i==9))).ToArray();
bool regressed=rollout.ApplyGuard(replay,args.Contains("problem"));
Console.WriteLine($"ROLLOUT baselineHelpful=10/10 candidateHelpful=3/10 candidateUnsafe=1 regressed={regressed} candidatePercent={rollout.CandidatePercent}");
Check.That(regressed&&rollout.CandidatePercent==0,"the agreed safety and quality guard disables the candidate","EVAL regressed candidate remained enabled");
string after=await alice.GetStringAsync("/version");Check.That(after=="baseline-v1","actual serving returns the baseline after rollback");
EvalData.Save(new {passed=true,actualHttp=true,stableVersion=first,feedbackCount=feedback.Count,regressed,candidatePercent=rollout.CandidatePercent,after,comparisonSource="explicit labelled replay",productionExperiment=false});
