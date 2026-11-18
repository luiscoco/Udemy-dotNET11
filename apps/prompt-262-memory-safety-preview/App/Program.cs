using System.Reflection;
string file="Consumer/Caller.cs";bool broken=args.Contains("problem");
try{
    File.WriteAllText(file,broken?"class Caller {public static int Run()=>NativeLike.ReadKnownValue();}":"class Caller {public static safe int Run(){unsafe{return NativeLike.ReadKnownValue();}}}");
    var built=await Harness.Run(Harness.Dotnet,"build","Consumer","-c","Release","--disable-build-servers","-p:UseSharedCompilation=false");
    Console.WriteLine(built.Text);
    Check.That(built.Code==0,"the actual compiler accepts the audited caller","PREVIEW memory-safety caller failed compilation");
    var executed=await Harness.Run(Harness.Dotnet,"run","--project","Consumer","-c","Release","--no-build");
    Console.WriteLine(executed.Text);Check.That(executed.Code==0&&executed.Text.Contains("value=42"),"the explicitly audited unsafe call executes");
    Check.That(executed.Text.Contains("MemorySafetyRulesAttribute"),"the actual compiler records the selected safety rules in assembly metadata");
    Harness.Save("preview-evidence.json",new{passed=true,actualCompiler=true,previewLanguage=true,updatedRules=true,unsafeCaller=true,value=42,metadata=true});
}finally{File.WriteAllText(file,"class Caller {public static safe int Run(){unsafe{return NativeLike.ReadKnownValue();}}}");}
