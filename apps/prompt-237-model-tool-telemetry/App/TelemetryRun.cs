using System.Diagnostics;
using Microsoft.Extensions.AI;
public static class TelemetryRun
{
    public static readonly ActivitySource Source=new("Course.AI.Evaluation");
    public static async Task<object> Run(EvalClient client,bool fabricate)
    {
        var spans=new List<string>();using var listener=new ActivityListener
        {
            ShouldListenTo=s=>s.Name=="Course.AI.Evaluation",Sample=(ref ActivityCreationOptions<ActivityContext> _)=>ActivitySamplingResult.AllData,
            ActivityStopped=a=>spans.Add(a.OperationName)
        };ActivitySource.AddActivityListener(listener);
        var watch=Stopwatch.StartNew();ChatResponse response;
        using(var root=Source.StartActivity("ai.request"))
        {
            root?.SetTag("model","qwen3:0.6b");
            using(var model=Source.StartActivity("model.generate"))
                response=await client.GetResponseAsync([new(ChatRole.User,"Say: backup retention is seven days. /no_think")]);
            using(var tool=Source.StartActivity("tool.read_policy")){tool?.SetTag("outcome","success");ReadPolicy();}
        }
        watch.Stop();
        long? input=response.Usage?.InputTokenCount,output=response.Usage?.OutputTokenCount;
        // No usage returned means unknown, never zero or an invented provider value.
        long? absent=UnknownUsage(fabricate);
        decimal? estimate=input.HasValue&&output.HasValue?(input.Value*0.10m+output.Value*0.20m)/1_000_000m:null;
        Console.WriteLine($"TELEMETRY elapsedMs={watch.Elapsed.TotalMilliseconds:F1} input={input} output={output} missingUsage={absent?.ToString()??"unknown"} estimateEUR={estimate}");
        Check.That(absent is null,"missing usage remains explicitly unknown","EVAL token usage was fabricated");
        Check.That(spans.Contains("ai.request")&&spans.Contains("model.generate")&&spans.Contains("tool.read_policy"),"actual root, model and tool spans reached the listener");
        return new {elapsedMs=watch.Elapsed.TotalMilliseconds,input,output,estimatedEUR=estimate,
            illustrativeInputRate=0.10m,illustrativeOutputRate=0.20m,ratePer="million tokens",isProviderInvoice=false,unknownUsage=absent,spans};
    }
    public static long? UnknownUsage(bool fabricate)=>fabricate?0:null;
    private static string ReadPolicy()=>"Synthetic retention: seven days.";
}
