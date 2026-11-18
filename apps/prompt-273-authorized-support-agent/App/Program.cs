using Microsoft.AspNetCore.Builder;
using System.Net.Http.Json;
using System.Text.Json;

using var model = AiRuntime.Create();
string proposal = await AiRuntime.Ask(model, "A customer requests an email update. Explain briefly why an approved tool needs authorization before a mutation. Do not execute anything.");
Console.WriteLine("SUPPORT untrusted model advisory: " + proposal);
Check.That(proposal.Length > 0, "actual local model advisory is received without granting authority");
bool broken = args.Contains("problem");
using var authority = new Authority();
var store = new Store("support");
var tools = new SupportTools(store, broken);
tools.Initialize();
var b = ApiSession.Builder();
authority.Configure(b.Services);
var app = b.Build();
app.UseAuthentication();
app.UseAuthorization();
tools.Map(app);
await using var api = await ApiSession.Start(app);
api.Token(authority.Issue(permission: "read"));
Check.That((await api.Client.GetAsync("/tools/lookup_customer")).IsSuccessStatusCode, "approved read tool executes");
Check.That((int)(await api.Client.GetAsync("/tools/arbitrary_shell")).StatusCode == 400, "unknown tool is rejected");
var request = new ChangeRequest("customer-1", "new@example.test", 1, "");
Check.That((int)(await api.Client.PostAsJsonAsync("/tools/set_contact_email", request)).StatusCode == 403, "read-only actor cannot mutate");
api.Token(authority.Issue());
var denied = await api.Client.PostAsJsonAsync("/tools/set_contact_email", request);
Console.WriteLine("SUPPORT no-approval status=" + (int)denied.StatusCode + " mutations=" + store.Scalar("SELECT COUNT(*) FROM Audit"));
Check.That((int)denied.StatusCode == 403 && store.Scalar("SELECT COUNT(*) FROM Audit") == 0, "mutation requires application approval even when model suggests a tool", "CAPSTONE support mutation approval failed");
// This explicit fixture action represents the authenticated human approval boundary.
var approved = await api.Client.PostAsJsonAsync("/approve", request);
using var json = JsonDocument.Parse(await approved.Content.ReadAsStringAsync());
string capability = json.RootElement.GetProperty("approval").GetString()!;
var tampered = request with
{
    Email = "tampered@example.test",
    Approval = capability
};
Check.That((int)(await api.Client.PostAsJsonAsync("/tools/set_contact_email", tampered)).StatusCode == 403, "approval is bound to exact arguments");
api.Token(authority.Issue(subject: "bob"));
Check.That((int)(await api.Client.PostAsJsonAsync("/tools/set_contact_email", request with { Approval = capability })).StatusCode == 403, "approval cannot be borrowed by another actor");
api.Token(authority.Issue());
var committed = await api.Client.PostAsJsonAsync("/tools/set_contact_email", request with { Approval = capability });
Check.That(committed.IsSuccessStatusCode, "approved version-checked mutation commits");
Check.That((int)(await api.Client.PostAsJsonAsync("/tools/set_contact_email", request with { Approval = capability })).StatusCode == 403 && store.Scalar("SELECT COUNT(*) FROM Audit") == 1, "single-use approval prevents replay and keeps one audit record");
Harness.Save("capstone-evidence.json", new { passed = true, actualModel = AiRuntime.Model, approvedTools = true, readOnlyRejected = true, noApprovalRejected = true, tamperRejected = true, otherActorRejected = true, replayRejected = true, auditRows = 1, modelAdvisory = proposal });
