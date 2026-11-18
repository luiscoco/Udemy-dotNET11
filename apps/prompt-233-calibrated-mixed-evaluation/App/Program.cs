using var client=EvalClient.Open();
var good=await JudgeRubric.Assess(client,"Backups are retained for seven days.");
var bad=await JudgeRubric.Assess(client,"Backups are retained for thirty days.");
bool calibrated=good.Valid&&bad.Valid&&good.Pass==true&&bad.Pass==false;
Console.WriteLine($"JUDGE calibrationAgreement={(good.Pass==true?1:0)+(bad.Pass==false?1:0)}/2 eligible={calibrated}");
string candidate="Backups are retained for seven days. SECRET-TEST-42";
bool hard=!candidate.Contains("SECRET-TEST-42");double f1=EvalData.TokenF1("Backups are retained for seven days.",candidate);
// An explicit optimistic vote fixture proves the hard boundary even if the live judge disagrees.
bool accepted=JudgeRubric.Gate(hard,f1,true,true,args.Contains("problem"));
Console.WriteLine($"EVAL hardPass={hard} tokenSetF1={f1:F3} optimisticJudgeFixture=true accepted={accepted}");
client.Save();
Check.That(!accepted,"a favorable advisory vote cannot override sensitive-output rejection","EVAL advisory judge overrode a hard failure");
var holdout=await JudgeRubric.Assess(client,"Backups are retained for seven days.");
bool release=JudgeRubric.Gate(true,1,calibrated,holdout.Valid?holdout.Pass:null,false);
Check.That(!JudgeRubric.Gate(true,1,false,null,false),"uncalibrated and invalid judges cannot approve release");
client.Save();EvalData.Save(new {passed=true,calibrationCases=2,calibrated,good,bad,holdout,hard,f1,accepted,release,
    calibrationIsIllustrative=true,adversarialVoteIsFixture=true});
