using Microsoft.AspNetCore.Builder;
using System.Text.Json;

bool broken = args.Contains("problem");
using var authority = new Authority();
var store = new Store("documents");
var documents = new Documents(store);
documents.Ingest();
var evaluations = new[]
{
    ("alpha", "alice", 1),
    ("alpha", "bob", 1),
    ("beta", "alice", 1),
    ("beta", "bob", 0)
};
foreach (var(tenant, reader, count)in evaluations)
{
    var rows = documents.Retrieve(tenant, reader, false);
    Check.That(rows.Count == count && rows.All(r => r.Tenant == tenant && r.Reader == reader), "retrieval evaluation " + tenant + "/" + reader + " respects both boundaries");
}

var b = ApiSession.Builder();
authority.Configure(b.Services);
var app = b.Build();
app.UseAuthentication();
app.UseAuthorization();
var assistant = new Assistant(documents, broken);
assistant.Map(app);
await using var api = await ApiSession.Start(app);
Check.That((int)(await api.Client.GetAsync("/ask")).StatusCode == 401, "assistant requires authentication");
api.Token(authority.Issue(permission: "read"));
using var response = await api.Client.GetAsync("/ask", HttpCompletionOption.ResponseHeadersRead);
response.EnsureSuccessStatusCode();
using var stream = new StreamReader(await response.Content.ReadAsStreamAsync());
string answer = "";
var citationIds = new List<string>();
while (await stream.ReadLineAsync()is string line)
{
    if (string.IsNullOrWhiteSpace(line))
        continue;
    using var doc = JsonDocument.Parse(line);
    var root = doc.RootElement;
    if (root.GetProperty("type").GetString() == "text")
        answer += root.GetProperty("text").GetString();
    if (root.GetProperty("type").GetString() == "citations")
        citationIds.AddRange(root.GetProperty("sources").EnumerateArray().Select(c => c.GetProperty("Id").GetString()!));
}

Console.WriteLine("ASSISTANT actual model stream: " + answer);
Console.WriteLine("ASSISTANT context=" + string.Join(',', assistant.LastContext.Select(c => c.Id)) + " updates=" + assistant.Updates);
Check.That(assistant.LastContext.All(c => c.Tenant == "alpha" && c.Reader == "alice") && citationIds.SequenceEqual(new[] { "guide" }), "only authorized chunks enter model context and citations", "CAPSTONE assistant permission filter failed");
Check.That(answer.Length > 0 && assistant.Updates > 0, "actual local model response arrives as streamed HTTP updates");
Harness.Save("capstone-evidence.json", new { passed = true, actualIngestion = true, actualSqlRetrieval = true, permissionEvaluations = 4, actualModel = AiRuntime.Model, actualStream = true, updates = assistant.Updates, citationIds, context = assistant.LastContext, answer, citationScope = "application-attached source provenance, not a semantic truth guarantee" });
