using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
bool broken = args.Contains("problem");
using var embeddings = new EmbeddingService();
await using var store = await VectorStore.Open(206);
await store.CreateCollection(); await store.Seed(embeddings);
using var chat = AiRuntime.Create();
var rag = new RagService(store,embeddings,chat,broken);
await using var web = await ApiHarness.StartAsync(_=>{}, app=>
{
    app.MapGet("/",()=>Results.Content(File.ReadAllText("rag-ui.html"),"text/html"));
    app.MapGet("/ask",async (string q)=>await rag.Answer(q));
    app.MapGet("/documents/{id}",async (string id)=>
    {
        var document = await store.Retrieve(id);
        if (document is null || document.Tenant != "a" || document.User != "alice") return Results.NotFound();
        return Results.Json(new SourceDocument(document.Id,document.Body,document.Version));
    });
});
if (args.Contains("serve")) { Console.WriteLine("RAG page: " + web.Client.BaseAddress); await web.App.WaitForShutdownAsync(); return; }
var supported = await web.Client.GetFromJsonAsync<RagAnswer>("/ask?q=rotate%20API%20keys");
Check.That(supported is { Abstained:false } && supported.Citations.Length == 1, "answerable request returns an actual source citation");
var source = Corpus.Documents.Single(d=>d.Id == supported!.Citations[0].Id);
Check.That(source.Body.Contains(supported!.Answer.TrimEnd('.')), "the returned quote occurs verbatim in the permitted source");
var resolved = await web.Client.GetFromJsonAsync<SourceDocument>(supported.Citations[0].Url);
Check.That(resolved!.Body.Contains(supported.Answer.TrimEnd('.')) && resolved.Version == supported.Citations[0].Version,
    "the citation resolves to its actual permitted source and version");
using var denied = await web.Client.GetAsync("/documents/tenant-b");
Check.That(denied.StatusCode == System.Net.HttpStatusCode.NotFound,"restricted source URLs are rejected independently");
int callsBefore = rag.ModelCalls;
var unknown = await web.Client.GetFromJsonAsync<RagAnswer>("/ask?q=purple%20moon%20cheese");
Console.WriteLine($"RAGAPI supportedCitations={supported.Citations.Length} unknownAbstained={unknown!.Abstained} unknownCitations={unknown.Citations.Length}");
Check.That(unknown.Abstained && unknown.Citations.Length == 0, "unsupported request abstains instead of inventing an answer", "RAG unsupported question received an uncited answer");
Check.That(rag.ModelCalls == callsBefore, "insufficient evidence does not trigger a generation fallback");
var browser = await Harness.Run("node","browser-driver.cjs",web.Client.BaseAddress!.ToString());
Console.WriteLine(browser.Text);
Check.That(browser.Code == 0,"the real browser displays citations and abstention");
AiRuntime.Evidence("rag-evidence.json",new { passed=true, supported, unknown, actualHttp=true, actualBrowser=true, citationResolved=true, restrictedSourcesRejected=true, modelCalls=rag.ModelCalls });
