using Microsoft.Extensions.AI;
var rows=EvalData.Load("data/evaluation-v1.jsonl");var row=rows.Single(r=>r.Id=="h1");
string[] retrieved=args.Contains("problem")?["unrelated-9"]:["backup-7"];
double recall=EvalData.Recall(row.RelevantIds,retrieved);
Console.WriteLine($"EVAL recallAt1={recall:F2} retrieved={string.Join(',',retrieved)}");
Check.That(recall==1,"reference document retrieval meets the agreed deterministic threshold","EVAL retrieval recall regressed");
using var client=EvalClient.Open();var scores=await QualityRun.Run(client);client.Save();
Check.That(client.Calls==4&&scores.Length==4,"four official .NET evaluators made actual model requests");
Console.WriteLine("EVAL model scores are advisory; invalid or inconclusive scores never become passes");
EvalData.Save(new {passed=true,actualOfficialEvaluators=true,modelCalls=client.Calls,recall,scores,
    candidate="authored reference fixture",judge="qwen3:0.6b",qualityCertified=false});
