using Npgsql;
using var oldModel = new EmbeddingService("all-minilm");
using var newModel = new EmbeddingService("nomic-embed-text");
await using var store = await VectorStore.Open(209);
await store.CreateCollection("items",384);
await store.CreateCollection("candidate",768);
await store.Seed(oldModel);
string oldDigest = await store.Scalar<string>("SELECT digest FROM items LIMIT 1");
await store.Execute("CREATE TABLE active_index(singleton int PRIMARY KEY,collection text,model text,dimensions int,digest text); INSERT INTO active_index VALUES(1,'items','all-minilm',384,@digest)",("digest",oldDigest));
var live = Corpus.Documents;
foreach (var document in args.Contains("problem") ? live.Take(1) : live)
{
    var embedded = await newModel.Generate(document.Title + ". " + document.Body);
    await store.Upsert(new(document,embedded),"candidate");
}
long candidateCount = await store.Scalar<long>("SELECT count(*) FROM candidate");
bool complete = candidateCount == live.Length;
string newDigest = await store.Scalar<string>("SELECT digest FROM candidate LIMIT 1");
if (complete || args.Contains("problem"))
    await store.Execute("UPDATE active_index SET collection='candidate',model='nomic-embed-text',dimensions=768,digest=@digest WHERE singleton=1",("digest",newDigest));
SearchProfile profile;
await using (var command = new NpgsqlCommand("SELECT collection,model,dimensions,digest FROM active_index WHERE singleton=1",store.Connection))
await using (var reader = await command.ExecuteReaderAsync())
{
    await reader.ReadAsync();
    profile = new(reader.GetString(0),reader.GetString(1),reader.GetInt32(2),reader.GetString(3));
}
Console.WriteLine($"CUTOVER candidateRows={candidateCount} required={live.Length} active={profile.Collection} complete={complete}");
Check.That(profile.Collection != "candidate" || complete,
    "activation requires a complete rebuilt index", "RAG incomplete candidate index activated");
using var activeModel = new EmbeddingService(profile.Model);
var query = await activeModel.Generate("rotate API keys",query:true);
Check.That(query.Stamp.Dimensions == profile.Dimensions && query.Stamp.Digest == profile.Digest,
    "one atomic profile binds collection, model, dimensions and actual model digest");
var hits = await store.Search(query.Vector,query.Text,table:profile.Collection);
Check.That(hits.Count > 0,"the selected rebuilt collection serves actual search results");
var bob = await store.Search(query.Vector,query.Text,user:"bob",table:profile.Collection);
var otherTenant = await store.Search(query.Vector,query.Text,tenant:"b",table:profile.Collection);
Check.That(bob.Count==1 && bob[0].Id=="bob-only" && otherTenant.Count==1, "the rebuilt collection preserves other actors and tenants");
bool mismatchRejected = false;
try
{
    var old = await oldModel.Generate("rotate API keys",query:true);
    await store.Search(old.Vector,old.Text,table:"candidate");
}
catch (PostgresException e) when (e.MessageText.Contains("dimensions")) { mismatchRejected = true; }
Check.That(mismatchRejected,"the database rejects old query vectors against the new dimension contract");
await store.Execute("UPDATE active_index SET collection='items',model='all-minilm',dimensions=384,digest=@digest WHERE singleton=1",("digest",oldDigest));
var rollbackQuery = await oldModel.Generate("rotate API keys",query:true);
var rollback = await store.Search(rollbackQuery.Vector,rollbackQuery.Text);
Check.That(rollback.Count > 0,"the retained old collection remains available for rollback");
AiRuntime.Evidence("rag-evidence.json",new { passed=true,candidateCount,required=live.Length,oldDimensions=384,newDimensions=768,mismatchRejected,rollbackVerified=true,allTenantsPreserved=true,profile });
public sealed record SearchProfile(string Collection,string Model,int Dimensions,string Digest);
