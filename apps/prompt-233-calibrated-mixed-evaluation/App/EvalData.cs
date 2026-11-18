using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
public sealed record EvalCase(string Id,string Split,string Category,string Question,string Reference,
    string[] RelevantIds,string[] Forbidden,bool Abstain);
public static class EvalData
{
    public static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web){WriteIndented=true};
    public static EvalCase[] Load(string file)
    {
        var rows=File.ReadLines(file).Where(l=>!string.IsNullOrWhiteSpace(l))
            .Select(l=>JsonSerializer.Deserialize<EvalCase>(l,Json)??throw new InvalidDataException("Null row")).ToArray();
        if(rows.Length<6||rows.Select(r=>r.Id).Distinct().Count()!=rows.Length
            ||rows.Any(r=>string.IsNullOrWhiteSpace(r.Question)||r.RelevantIds is null||r.Forbidden is null
                ||r.Split is not ("calibration" or "holdout")))throw new InvalidDataException("Invalid evaluation dataset");
        if(rows.GroupBy(r=>r.Question.Trim().ToLowerInvariant()).Any(g=>g.Select(r=>r.Split).Distinct().Count()>1))
            throw new InvalidDataException("Cross-split duplicate question");
        return rows;
    }
    public static string Hash(string file)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
    public static double Recall(string[] relevant,string[] retrieved)=>relevant.Length==0?0:
        relevant.Intersect(retrieved,StringComparer.Ordinal).Count()/(double)relevant.Distinct().Count();
    public static double TokenF1(string reference,string candidate)
    {
        static HashSet<string> Tokens(string s)=>System.Text.RegularExpressions.Regex.Matches(s.ToLowerInvariant(),@"[a-z0-9]+")
            .Select(m=>m.Value).ToHashSet();
        var a=Tokens(reference);var b=Tokens(candidate);int shared=a.Intersect(b).Count();
        return a.Count+b.Count==0?1:2.0*shared/(a.Count+b.Count);
    }
    public static void Save(object value)=>Harness.Save("eval-evidence.json",value);
}
