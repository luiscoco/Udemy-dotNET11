using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
using var classifierClient=AgentRuntime.Client();
var options=AgentRuntime.Options("Classify billing invoice questions as Billing and software debugging as Technical. Return JSON with Route exactly Billing or Technical.");
options.ResponseFormat=ChatResponseFormat.ForJsonSchema<RouteChoice>();
var classifier=classifierClient.AsAIAgent(new ChatClientAgentOptions { Name="Router",ChatOptions=options });
var request=new ServiceRequest("task-7","a","alice","Explain the invoice total",
    "Invoice total is 18 EUR.","Build number is 42.","UNRELATED-PRIVATE-A7");
string decision=await AgentRuntime.Ask(classifier,request.Topic);
var selected=HandoffPolicy.Parse(JsonSerializer.Deserialize<RouteChoice>(decision,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!.Route);
Check.That(selected==Route.Billing,"the actual classifier chooses the billing specialist");
bool unknownRejected=false; try { HandoffPolicy.Parse("Unknown"); } catch(ArgumentException) { unknownRejected=true; }
Check.That(unknownRejected,"unknown route labels are rejected instead of selecting arbitrary tools");
string context=HandoffPolicy.Build(request,selected,args.Contains("problem"));
HandoffPolicy.AuthorizeReceiver(selected,request.Tenant,request.Actor);
using var specialistClient=AgentRuntime.Client();
var specialist=AgentRuntime.Create(specialistClient,"BillingSpecialist","Explain only the supplied invoice facts in one sentence.");
var session=await specialist.CreateSessionAsync();
string answer=await AgentRuntime.Ask(specialist,context,session);
bool minimized=!specialistClient.InputText.Contains("UNRELATED-PRIVATE-A7")&&!specialistClient.InputText.Contains("Build number");
Console.WriteLine($"HANDOFF route={selected} minimized={minimized} task={request.TaskId}");
Check.That(minimized,"receiving model input contains only authorized relevant facts","AGENT handoff leaked unrelated context");
Check.That(specialistClient.InputText.Contains("18 EUR")&&specialistClient.InputText.Contains("task-7"),"handoff preserves the task identifier and needed evidence");
bool denied=false; try { HandoffPolicy.AuthorizeReceiver(selected,"b","alice"); } catch(UnauthorizedAccessException) { denied=true; }
Check.That(denied,"the receiving boundary rechecks resource authorization");
AgentRuntime.Save(new { passed=true,route=selected.ToString(),minimized,unknownRejected,receiverDenied=denied,answer,hops=1 });
