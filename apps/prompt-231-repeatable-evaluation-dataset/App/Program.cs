using System.Text.Json;
string file="data/evaluation-v1.jsonl";
var rows=EvalData.Load(file);string hash=EvalData.Hash(file);
Check.That(hash==EvalData.Hash(file),"the immutable input bytes retain their SHA256 identity");
Check.That(rows.Select(r=>r.Id).Distinct().Count()==rows.Length,"case identifiers are unique");
var holdout=rows.Where(r=>r.Split=="holdout").ToArray();
if(args.Contains("problem"))holdout=holdout.Where(r=>r.Category=="answerable").ToArray();
string[] required=["answerable","missing-context","injection"];
var categories=holdout.Select(r=>r.Category).Distinct().ToArray();
Console.WriteLine($"EVAL dataset=v1 hash={hash[..12]} holdout={holdout.Length} categories={string.Join(',',categories)}");
Check.That(required.All(categories.Contains),"the held-out set retains representative failure categories","EVAL failure categories were omitted");
var first=JsonSerializer.Serialize(holdout.Select(DatasetRunner.Evaluate),EvalData.Json);
var second=JsonSerializer.Serialize(holdout.Select(DatasetRunner.Evaluate),EvalData.Json);
Check.That(first==second,"the deterministic candidate fixture produces repeatable case results");
bool duplicateRejected=false;string invalid=Path.Combine("artifacts","invalid-dataset.jsonl");Directory.CreateDirectory("artifacts");
File.WriteAllText(invalid,File.ReadAllText(file)+File.ReadLines(file).First()+Environment.NewLine);
try{EvalData.Load(invalid);}catch(InvalidDataException){duplicateRejected=true;}
Check.That(duplicateRejected,"the loader rejects duplicate identifiers before evaluation");
EvalData.Save(new {passed=true,datasetVersion="v1",sha256=hash,holdout=holdout.Length,categories,duplicateRejected,
    deterministicCandidate=true,modelInvoked=false,results=JsonSerializer.Deserialize<JsonElement>(first)});
