using System.Runtime.InteropServices;
using System.Text.Json;
var samples = new List<object>();
var medians = new Dictionary<string,double>();
bool problem = args.FirstOrDefault() == "problem";
foreach (string target in new[]{"net10.0","net11.0"})
{
    var publish = await Harness.Run(Harness.Dotnet,"publish","Workload","-c","Release","-f",target,"-r","win-x64","--self-contained","true","-o","artifacts/published/"+target);
    File.WriteAllText("artifacts/publish-"+target+".txt",publish.Text);
    Check.That(publish.Code == 0,"published "+target+" under identical deployment settings");
}
var elapsed = new Dictionary<string,List<double>> { ["net10.0"]=[], ["net11.0"]=[] };
for(int repeat=0;repeat<7;repeat++)
{
    string[] order = repeat % 2 == 0 ? ["net10.0","net11.0"] : ["net11.0","net10.0"];
    foreach(string target in order)
    {
        var result = await Harness.Run(Path.GetFullPath("artifacts/published/"+target+"/Workload.exe"), problem && target == "net11.0" ? "wrong-input" : "same-input");
        Check.That(result.Code == 0,"workload process completed "+target);
        using var json = JsonDocument.Parse(result.Text);
        int length=json.RootElement.GetProperty("length").GetInt32();
        long checksum=json.RootElement.GetProperty("checksum").GetInt64();
        Console.WriteLine($"{target} run {repeat+1}, length={length}, checksum={checksum}");
        Check.That(length == 1000000 && checksum == 1020000000L,"INPUT_PARITY_REGRESSION identical input and result");
        double ms=json.RootElement.GetProperty("milliseconds").GetDouble();
        elapsed[target].Add(ms);
        samples.Add(new { target, repeat, observation=json.RootElement.Clone() });
    }
}
foreach(var pair in elapsed)
{
    double median=pair.Value.Order().ElementAt(pair.Value.Count/2);
    medians[pair.Key]=median;
    Console.WriteLine($"{pair.Key} median={median:F2} ms, min={pair.Value.Min():F2}, max={pair.Value.Max():F2}, n=7");
}
Harness.Save("framework-benchmark.json",new { os=RuntimeInformation.OSDescription, architecture=RuntimeInformation.ProcessArchitecture.ToString(), sdk="11.0.100", deployment="Release win-x64 self-contained", samples, medians });
Check.That(samples.Count == 14,"retained all fresh-process samples");
