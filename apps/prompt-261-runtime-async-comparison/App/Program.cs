using System.Text.Json;
async Task<JsonElement> Execute(string variant,bool problem){
    var result=await Harness.Run(Harness.Dotnet,"run","--project",variant,"-c","Release","--no-build","--",problem?"problem":"verify");
    Check.That(result.Code==0,"the "+variant+" worker executed");return JsonDocument.Parse(result.Text).RootElement.Clone();
}
var classic=await Execute("Classic",false);var runtime=await Execute("Runtime",args.Contains("problem"));
foreach(var pair in new[]{("classic",classic),("runtime",runtime)}){Console.WriteLine($"ASYNC {pair.Item1} medianMs={pair.Item2.GetProperty("medianMs")} runtimeAsync={pair.Item2.GetProperty("runtimeAsync")} stateMachine={pair.Item2.GetProperty("stateMachine")}");Console.WriteLine(string.Join(Environment.NewLine,pair.Item2.GetProperty("trace").GetString()!.Split(Environment.NewLine).Take(6)));}
Check.That(classic.GetProperty("sum").GetInt64()==499500&&runtime.GetProperty("sum").GetInt64()==499500,"both compilation models preserve the workload result");
Check.That(runtime.GetProperty("runtimeAsync").GetBoolean()&&!classic.GetProperty("runtimeAsync").GetBoolean(),"actual method metadata distinguishes runtime async from compiler state machines");
Check.That(classic.GetProperty("observed").GetBoolean()&&runtime.GetProperty("observed").GetBoolean(),"both compilation variants preserve cancellation","PREVIEW async cancellation compatibility failed");
Harness.Save("preview-evidence.json",new{passed=true,actualRuntimeAsync=true,classic,runtime,performanceClaim="local descriptive measurements only"});
