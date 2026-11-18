using Microsoft.AspNetCore.Builder;
using System.Diagnostics.Metrics;
using System.Net.Http.Json;

bool broken = args.Contains("problem");
long measured = 0;
using var listener = new MeterListener();
listener.InstrumentPublished = (i, l) =>
{
    if (i.Meter.Name == "Course.Saas")
        l.EnableMeasurementEvents(i);
};
listener.SetMeasurementEventCallback<long>((i, value, tags, state) => Interlocked.Add(ref measured, value));
listener.Start();
using var authority = new Authority();
var store = new Store("saas");
using var saas = new SaasApi(store, broken);
var b = ApiSession.Builder();
authority.Configure(b.Services);
saas.Configure(b);
var app = b.Build();
app.UseAuthentication();
app.UseAuthorization();
await saas.Map(app);
await using var api = await ApiSession.Start(app);
api.Token(authority.Issue());
foreach (int n in new[]
{
    1,
    2
}

)
    Check.That((int)(await api.Client.PostAsJsonAsync("/documents", new NewDocument("alpha-" + n, "alpha data"))).StatusCode == 201, "alpha document " + n + " consumes one quota unit");
Check.That((int)(await api.Client.PostAsJsonAsync("/documents", new NewDocument("alpha-3", "over quota"))).StatusCode == 429, "alpha quota rejects third document");
api.Token(authority.Issue(subject: "bob", tenant: "beta"));
Check.That((int)(await api.Client.PostAsJsonAsync("/documents", new NewDocument("beta-1", "tenant beta private"))).StatusCode == 201, "beta has independent quota");
api.Token(authority.Issue());
api.Client.DefaultRequestHeaders.Add("X-Tenant", "beta");
var cross = await api.Client.GetAsync("/documents/beta-1");
Console.WriteLine("SAAS cross-tenant status=" + (int)cross.StatusCode + " measuredRequests=" + measured);
Check.That((int)cross.StatusCode == 404, "EF tenant filter rejects cross-tenant access despite spoofed header", "CAPSTONE SaaS tenant isolation failed");
var audit = await api.Client.GetFromJsonAsync<string[]>("/audit");
Check.That(audit!.Length == 2 && audit.All(x => x.StartsWith("alpha-")), "tenant-scoped auditing excludes other tenant records");
Check.That(store.Scalar("SELECT Used FROM Usage WHERE Tenant='alpha'") == 2 && store.Scalar("SELECT COUNT(*) FROM Audit") == 3 && measured == 4, "quota, documents, audit and observed meter agree");
Harness.Save("capstone-evidence.json", new { passed = true, actualEfFilter = true, actualSqlQuota = true, spoofedTenantRejected = true, alphaQuota = 2, betaQuota = 1, auditRows = 3, observedMeasurements = measured, allowed = saas.Allowed, denied = saas.Denied });
