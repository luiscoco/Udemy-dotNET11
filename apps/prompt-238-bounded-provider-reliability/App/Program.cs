using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
var admission=new RequestAdmission();
Check.That(admission.Admit("tenant-a","r1")&&!admission.Admit("tenant-a","overflow"),"the actual bounded channel refuses overflow without spending quota");
Check.That(await admission.Queue.Reader.ReadAsync()=="r1"&&admission.Admit("tenant-a","r2"),"the consumer frees exactly one queue slot");
await admission.Queue.Reader.ReadAsync();Check.That(!admission.Admit("tenant-a","quota-3"),"per-tenant quota rejects the third admitted request");
var builder=WebFixture.Builder();var app=builder.Build();int primaryCalls=0,alternateCalls=0;
app.MapGet("/primary",()=>{primaryCalls++;return Results.StatusCode(503);});
app.MapGet("/unauthorized",()=>Results.StatusCode(401));app.MapGet("/recovered",()=>Results.Text("fixture recovered"));
app.MapGet("/alternate",()=>{alternateCalls++;return Results.Text("explicit synthetic provider fixture");});
await using var server=await WebFixture.Start(app);using var http=server.Client();
var policy=new ProviderPolicy(http,args.Contains("problem"));
var denied=await policy.Send("/unauthorized");
Console.WriteLine($"QUEUE unauthorized={(int)denied} fallbackCount={policy.Fallbacks} alternateCalls={alternateCalls}");
Check.That(denied==HttpStatusCode.Unauthorized&&policy.Fallbacks==0,"authorization failures never trigger provider fallback","EVAL authorization failure triggered fallback");
Check.That(await policy.Send("/primary")==HttpStatusCode.OK&&await policy.Send("/primary")==HttpStatusCode.OK,"eligible transient reads use one bounded alternate attempt each");
bool circuit=false;try{await policy.Send("/primary");}catch(InvalidOperationException e)when(e.Message=="Circuit open"){circuit=true;}
Check.That(circuit&&primaryCalls==2&&alternateCalls==2,"the open circuit prevents another primary request");
using var cancelled=new CancellationTokenSource();cancelled.Cancel();bool cancelledEarly=false;
try{await policy.Send("/primary",cancellationToken:cancelled.Token);}catch(OperationCanceledException){cancelledEarly=true;}
Check.That(cancelledEarly&&primaryCalls==2,"cancellation does not consume another provider attempt");
var probe=await http.GetAsync("/recovered");policy.HalfOpenAfterRecoveryProbe(elapsedCooldown:true,probe.IsSuccessStatusCode);
Check.That(!policy.CircuitOpen,"an explicitly controlled cooldown and real successful probe close the circuit");
var incompatible=new ProviderPolicy(http,false);Check.That(await incompatible.Send("/primary",compatible:false)==HttpStatusCode.ServiceUnavailable&&incompatible.Fallbacks==0,"incompatible model contracts cannot silently fall back");
EvalData.Save(new {passed=true,realHttp=true,syntheticProviders=true,queueCapacity=1,tenantQuota=2,primaryCalls,alternateCalls,policy.Attempts,policy.Fallbacks,cancelledEarly,circuitRecovered=!policy.CircuitOpen});
