using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Course.Analyzers;
string api="namespace Course.Payments {public static class Gateway {public static void Charge(decimal amount,string idempotencyKey){} }}";
string[] bodies=["""Course.Payments.Gateway.Charge(12m,"receipt-7");""",
    """Course.Payments.Gateway.Charge(idempotencyKey: "",amount: 12m);""",
    """Course.Payments.Gateway.Charge(12m,"   ");"""];
var counts=new List<int>();
foreach(string body in bodies)
{
    var compilation=CompilerInput.Create(api+"public class Test {public void Run(){"+body+"}}");
    var diagnostics=await compilation.WithAnalyzers([new PaymentKeyAnalyzer()]).GetAnalyzerDiagnosticsAsync();
    counts.Add(diagnostics.Count(d=>d.Id=="PAY251"));
}
Check.That(counts.SequenceEqual(new[]{0,1,1}),"actual semantic analysis distinguishes valid, empty named and whitespace keys");
var other=CompilerInput.Create("""class Gateway {public static void Charge(decimal amount,string idempotencyKey){} } class T {void M(){Gateway.Charge(1m,"");}}""");
Check.That((await other.WithAnalyzers([new PaymentKeyAnalyzer()]).GetAnalyzerDiagnosticsAsync()).Length==0,"a same-named method on another type is not falsely diagnosed");
string probe="Sample/UnsafePayment.cs";
try
{
    File.WriteAllText(probe,args.Contains("problem")?"""class UnsafePayment {void Run()=>Course.Payments.Gateway.Charge(12m,"");}""":"// Valid probe restored.");
    var built=await Harness.Run(Harness.Dotnet,"build","Sample","-c","Release","--disable-build-servers","-p:UseSharedCompilation=false");
    Console.WriteLine(built.Text);Console.WriteLine($"ANALYZER diagnosticCounts={string.Join(',',counts)} sampleBuildExit={built.Code}");
    Check.That(built.Code==0,"the actual compiler accepts the safe payment sample","EXT payment analyzer rejected unsafe code");
    var run=await Harness.Run(Harness.Dotnet,"run","--project","Sample","-c","Release","--no-build");
    Check.That(run.Code==0&&run.Text.Contains("receipt-7"),"the actual compiled consumer executes with a stable idempotency key");
    Harness.Save("ext-evidence.json",new {passed=true,actualRoslyn=true,diagnostic="PAY251",counts,falsePositiveRejected=true,sampleExit=run.Code});
}
finally{File.WriteAllText(probe,"// Valid probe restored.");}
