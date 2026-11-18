using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Course.Generators;
string prefix="namespace Course { [System.AttributeUsage(System.AttributeTargets.Class)] public sealed class GenerateEventAttribute:System.Attribute {} }";
string type=args.Contains("problem")?"[Course.GenerateEvent] public partial class StockAdjusted {}":
    """[Course.GenerateEvent] public partial class StockAdjusted {public string Id {get;set;}="event-7";}""";
var compilation=CompilerInput.Create(prefix+type);
GeneratorDriver driver=CSharpGeneratorDriver.Create(generators:new[]{new EventGenerator().AsSourceGenerator()},
    driverOptions:new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None,trackIncrementalGeneratorSteps:true));
driver=driver.RunGeneratorsAndUpdateCompilation(compilation,out var generated,out var diagnostics);
Console.WriteLine("GENERATOR diagnostics="+string.Join(',',diagnostics.Select(d=>d.Id)));
Check.That(!diagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error),"the generator accepts the validated event shape","EXT source generator rejected an invalid contract");
var result=driver.GetRunResult();string source=result.GeneratedTrees.Single().ToString();
Check.That(source.Contains("EventName")&&source.Contains("Describe"),"real generated source contains the expected public contract");
driver=driver.RunGeneratorsAndUpdateCompilation(compilation,out _,out _);
var reasons=driver.GetRunResult().Results.Single().TrackedSteps["EventContracts"].SelectMany(s=>s.Outputs).Select(o=>o.Reason.ToString()).ToArray();
Check.That(reasons.All(r=>r is "Cached" or "Unchanged"),"the tracked incremental node is reused on an unchanged compilation");
using var stream=new MemoryStream();var emitted=generated.Emit(stream);Check.That(emitted.Success,"the generated compilation emits actual runnable IL");
var assembly=System.Reflection.Assembly.Load(stream.ToArray());var eventType=assembly.GetType("StockAdjusted")!;
var instance=Activator.CreateInstance(eventType);string description=(string)eventType.GetMethod("Describe")!.Invoke(instance,null)!;
Check.That(description=="StockAdjusted:event-7","the compiled generated member executes with the source event identifier");
Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/generated-event.cs",source);
Harness.Save("ext-evidence.json",new {passed=true,actualGenerator=true,generatedSources=result.GeneratedTrees.Length,reasons,description,emitted=emitted.Success});
