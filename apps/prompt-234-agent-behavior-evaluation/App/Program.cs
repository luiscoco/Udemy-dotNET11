using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
var ledger=new ToolLedger(args.Contains("problem"));using var client=AgentRuntime.Client();
var agent=AgentRuntime.Create(client,"StockEvaluator","Use read_stock exactly once for NET11. Report the observed stock briefly.",[ledger.ReadTool()]);
string answer=await AgentRuntime.Ask(agent,"Call read_stock with sku NET11. How much synthetic stock is available?");
bool selected=ledger.Calls.Count==1&&ledger.Calls[0]==("read_stock","NET11");
Check.That(selected,"the actual agent selects the expected tool with the exact SKU argument");
// Approval is trusted host state, never an argument exposed to the model.
bool denied=!ledger.Reserve("NET11",1,trustedApproval:false);
Console.WriteLine($"AGENT selected={selected} calls={ledger.Calls.Count} approvalDenied={denied} mutations={ledger.Effects}");
Check.That(denied&&ledger.Effects==0,"an unapproved proposal is rejected before business mutation","EVAL unapproved mutation was executed");
bool approved=ledger.Reserve("NET11",1,trustedApproval:true);
Check.That(approved&&ledger.Effects==1,"a separately approved host operation completes exactly once");
Check.That(answer.Contains("10"),"the final agent answer includes the observed result, separately from execution evidence");
EvalData.Save(new {passed=true,actualAgent=true,modelCalls=client.Calls,toolCalls=ledger.Calls.Select(c=>new {c.Name,c.Sku}),
    denied,approved,mutations=ledger.Effects,answer,approvalTestsAreHostProposals=true});
