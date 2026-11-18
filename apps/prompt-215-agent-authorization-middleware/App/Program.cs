using Microsoft.Agents.AI;
using var client=AgentRuntime.Client();
AIAgent core=AgentRuntime.Create(client,"GuardedTutor","Explain C# concepts briefly.");
var viewerGuard=new RunGuard(false,args.Contains("problem"));
var viewer=core.AsBuilder().Use(viewerGuard.Invoke,null).Build();
bool denied=false; try { await AgentRuntime.Ask(viewer,"Explain a stream in one sentence."); }
catch(UnauthorizedAccessException) { denied=true; }
Console.WriteLine($"MIDDLEWARE unauthorizedDenied={denied} underlyingCalls={client.Calls}");
Check.That(denied&&client.Calls==0,"authorization rejects before model work","AGENT middleware permitted unauthorized run");
var guard=new RunGuard(true); var allowed=core.AsBuilder().Use(guard.Invoke,null).Build();
bool invalid=false; try { await AgentRuntime.Ask(allowed,new string('x',201)); } catch(ArgumentException) { invalid=true; }
Check.That(invalid&&guard.Executions==0,"input validation rejects before execution");
string answer=await AgentRuntime.Ask(allowed,"Explain a stream in one sentence.");
bool budget=false; try { await AgentRuntime.Ask(allowed,"Explain another stream."); }
catch(InvalidOperationException e) when(e.Message.Contains("budget")) { budget=true; }
Check.That(budget&&guard.Executions==1,"middleware enforces the one-run request budget");
Console.WriteLine("MIDDLEWARE safe events="+string.Join(",",guard.Events));
Check.That(guard.Events.All(e=>!e.Contains("stream")),"telemetry stores run identifiers and status without raw input");
AgentRuntime.Save(new { passed=true,denied,invalid,budget,guard.Executions,guard.Events,answer });
