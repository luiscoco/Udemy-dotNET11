var cases=EvalData.Load("data/evaluation-v1.jsonl").Where(r=>r.Split=="holdout").ToArray();
// Scores are explicit deterministic replay inputs for the CI mechanism, not generated model scores.
CaseScore[] baseline=cases.Select(r=>new CaseScore(r.Id,true,1,1)).ToArray();
Check.That(QualityGate.Accept(baseline,cases.Select(r=>r.Id).ToArray()),"the known baseline passes every agreed per-case threshold");
var missing=baseline[..^1];Check.That(!QualityGate.Accept(missing,cases.Select(r=>r.Id).ToArray()),"missing cases cannot improve an aggregate by disappearing");
var unknown=baseline.ToArray();unknown[0]=unknown[0] with {Groundedness=null};
Check.That(!QualityGate.Accept(unknown,cases.Select(r=>r.Id).ToArray()),"missing metric values fail closed");
var candidate=baseline.ToArray();if(args.Contains("problem"))candidate[0]=candidate[0] with {HardPass=false,Groundedness=0.2};
bool accepted=QualityGate.Accept(candidate,cases.Select(r=>r.Id).ToArray());
QualityGate.Report(candidate,accepted,EvalData.Hash("data/evaluation-v1.jsonl"));
Console.WriteLine($"GATE cases={candidate.Length} accepted={accepted} report=artifacts/quality-report.html");
if(!accepted){Console.Error.WriteLine("EVAL CI quality gate rejected regression");Environment.ExitCode=2;return;}
Check.That(File.Exists("artifacts/quality-report.json")&&File.Exists("artifacts/quality-report.html"),"machine-readable and human-readable reports were generated");
EvalData.Save(new {passed=true,accepted,report="artifacts/quality-report.json",controlledRegressionExit=2,source="explicit deterministic replay",cases=candidate.Length});
