using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Text.Json;
public static class OnnxComparison
{
    public static (double Ms,int Digit,float[] Scores,string Profile) Run(bool dml,byte[] input,bool wrongRank)
    {
        Directory.CreateDirectory("artifacts");using var options=new SessionOptions
        {EnableMemoryPattern=false,ExecutionMode=ExecutionMode.ORT_SEQUENTIAL,EnableProfiling=true,
            ProfileOutputPathPrefix=Path.GetFullPath("artifacts/"+(dml?"dml":"cpu"))};
        if(dml){options.AddSessionConfigEntry("ep.dml.disable_graph_fusion","1");options.AppendExecutionProvider_DML(0);}
        using var session=new InferenceSession("assets/mnist-12.onnx",options);
        var metadata=session.InputMetadata.Single();
        Console.WriteLine($"ONNX requested={(dml?"DirectML":"CPU")} input={metadata.Key} shape={string.Join('x',metadata.Value.Dimensions)}");
        try{DigitModel.Infer(session,input,wrongRank);}catch(OnnxRuntimeException e)when(wrongRank)
            {throw new InvalidOperationException("ONNX input rank mismatch",e);}
        var result=DigitModel.Infer(session,input);var watch=Stopwatch.StartNew();
        for(int i=0;i<20;i++)result=DigitModel.Infer(session,input);
        watch.Stop();string profile=session.EndProfiling();
        return (watch.Elapsed.TotalMilliseconds/20,result.Digit,result.Scores,profile);
    }
    public static int DmlNodes(string file)
    {
        using var json=JsonDocument.Parse(File.ReadAllText(file));
        return json.RootElement.EnumerateArray().Count(e=>e.TryGetProperty("args",out var a)
            &&a.TryGetProperty("provider",out var p)&&p.GetString()=="DmlExecutionProvider");
    }
}
