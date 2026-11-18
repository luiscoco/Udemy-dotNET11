using System.Text.Json;
using System.Net;
public sealed record CaseScore(string Id,bool HardPass,double? Groundedness,double? Recall);
public static class QualityGate
{
    public static bool Accept(CaseScore[] scores,string[] expected)
    {
        if(scores.Length!=expected.Length||scores.Select(s=>s.Id).Distinct().Count()!=scores.Length
            ||!expected.All(id=>scores.Any(s=>s.Id==id)))return false;
        return scores.All(s=>s.HardPass&&s.Groundedness is >=0.8 and <=1&&s.Recall is >=0.8 and <=1);
    }
    public static void Report(CaseScore[] scores,bool accepted,string hash)
    {
        Directory.CreateDirectory("artifacts");
        File.WriteAllText("artifacts/quality-report.json",JsonSerializer.Serialize(new {datasetHash=hash,accepted,scores},EvalData.Json));
        var rows=string.Join("",scores.Select(s=>$"<tr><td>{WebUtility.HtmlEncode(s.Id)}</td><td>{s.HardPass}</td><td>{s.Groundedness}</td><td>{s.Recall}</td></tr>"));
        File.WriteAllText("artifacts/quality-report.html",$"<!doctype html><meta charset=utf-8><title>Evaluation gate</title><h1>Release accepted: {accepted}</h1><p>Explicit deterministic replay fixture; no model judge claimed.</p><table><tr><th>Case</th><th>Hard pass</th><th>Groundedness</th><th>Recall</th></tr>{rows}</table>");
    }
}
