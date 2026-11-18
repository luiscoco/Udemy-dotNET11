using Microsoft.ML.OnnxRuntime;
var input=File.ReadAllBytes("assets/digit-7.bin");
var cpu=OnnxComparison.Run(false,input,args.Contains("problem"));
Check.That(cpu.Digit==7,"the actual ONNX CPU model recognizes the supplied held-out digit");
bool accelerated=false,numericallyValid=false,nonFiniteOutputs=false;string? unavailable=null;double? dmlMs=null;int dmlNodes=0;double? maxDifference=null;int? dmlDigit=null;string? rawDifference=null;
string selectedProvider="CPU";int selectedDigit=cpu.Digit;
try
{
    var dml=OnnxComparison.Run(true,input,false);dmlNodes=OnnxComparison.DmlNodes(dml.Profile);accelerated=dmlNodes>0;
    double measuredDifference=cpu.Scores.Zip(dml.Scores,(a,b)=>(double)Math.Abs(a-b)).Max();dmlMs=dml.Ms;
    nonFiniteOutputs=dml.Scores.Any(s=>!float.IsFinite(s));rawDifference=measuredDifference.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
    maxDifference=double.IsFinite(measuredDifference)?measuredDifference:null;
    dmlDigit=dml.Digit;numericallyValid=!nonFiniteOutputs&&dml.Digit==cpu.Digit&&maxDifference<0.01;
    Console.WriteLine($"ONNX cpuDigit={cpu.Digit} dmlDigit={dml.Digit} maximumScoreDifference={maxDifference} dmlNodes={dmlNodes} numericalCheck={numericallyValid}");
    if(accelerated&&numericallyValid){selectedProvider="DirectML";selectedDigit=dml.Digit;}
    else Console.WriteLine("ONNX DirectML result rejected; use the validated CPU result. No acceleration quality or speedup claim.");
}
catch(OnnxRuntimeException e){unavailable=e.Message;}
catch(DllNotFoundException e){unavailable=e.Message;}
Console.WriteLine($"ONNX cpuMs={cpu.Ms:F4} dmlMs={dmlMs:F4} observedDmlNodes={dmlNodes} accelerated={accelerated} maxDifference={maxDifference}");
if(unavailable!=null)Console.WriteLine("ONNX acceleration unavailable: "+unavailable);
Check.That(selectedDigit==7&&(selectedProvider=="CPU"||numericallyValid),"only a numerically validated result is returned to the application");
var digest=System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("assets/mnist-12.onnx"));
Harness.Save("ml-evidence.json",new {passed=true,actualOnnx=true,cpuDigit=cpu.Digit,cpuMs=cpu.Ms,dmlMs,dmlNodes,accelerated,maxDifference,unavailable,
    dmlDigit,numericallyValid,nonFiniteOutputs,rawDifference,selectedProvider,selectedDigit,speedupClaim=false,modelSha256=Convert.ToHexString(digest)});
