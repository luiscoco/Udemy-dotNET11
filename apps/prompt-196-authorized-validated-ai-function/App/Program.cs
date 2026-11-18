using Microsoft.Extensions.AI;
using System.Security.Claims;
static ClaimsPrincipal Actor(bool writer) => new(new ClaimsIdentity(
    writer ? new[] { new Claim(ClaimTypes.Role, "stock-writer") } : [], "fixture"));
var denied = new InventoryTool(Actor(false), !args.Contains("problem"));
var function = denied.Export();
bool forbidden = false;
try { await function.InvokeAsync(new AIFunctionArguments { ["quantity"] = 2 }); }
catch (Exception e) when (e.ToString().Contains("Stock writer role required")) { forbidden = true; }
Console.WriteLine($"AUTH viewer denied={forbidden} sideEffects={denied.Executions}");
AiRuntime.Evidence("authorization-boundary.json", new { forbidden, denied.Executions });
Check.That(forbidden && denied.Executions == 0, "unauthorized actor is denied before side effects", "AI unauthorized tool execution");
var allowed = new InventoryTool(Actor(true), true);
var exported = allowed.Export();
var receipt = await exported.InvokeAsync(new AIFunctionArguments { ["quantity"] = 2 });
bool invalidRejected = false;
try { await exported.InvokeAsync(new AIFunctionArguments { ["quantity"] = 99 }); }
catch { invalidRejected = true; }
Check.That(invalidRejected && allowed.Executions == 1, "authorized execution validates arguments before side effects");
Console.WriteLine("AUTH " + receipt);
using var chat = AiRuntime.Create();
string text = await AiRuntime.Ask(chat, "Explain why an application must authorize inventory changes in one sentence.");
AiRuntime.Evidence("ai-evidence.json", new { passed = true, forbidden, invalidRejected, allowed.Executions, text, modelToolCalling = false });
