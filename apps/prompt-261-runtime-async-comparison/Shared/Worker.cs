using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
bool ignoreCancellation=args.Contains("problem");
var measurements=new List<double>();long sum=0;
for(int sample=0;sample<6;sample++){
    var watch=Stopwatch.StartNew();sum=await Work.SumAsync(1000);watch.Stop();
    if(sample>0)measurements.Add(watch.Elapsed.TotalMilliseconds);
}
using var cancelled=new CancellationTokenSource();cancelled.Cancel();bool observed=false;
try{await Work.CancelAsync(cancelled.Token,ignoreCancellation);}catch(OperationCanceledException){observed=true;}
var method=typeof(Work).GetMethod(nameof(Work.SumAsync))!;
Console.WriteLine(JsonSerializer.Serialize(new{sum,observed,flags=method.GetMethodImplementationFlags().ToString(),stateMachine=method.GetCustomAttribute<AsyncStateMachineAttribute>()!=null,runtimeAsync=(method.GetMethodImplementationFlags()&MethodImplAttributes.Async)!=0,
    medianMs=measurements.Order().ElementAt(2),samples=measurements,trace=await Work.OuterAsync()}));
public static class Work
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<long> SumAsync(int count){long sum=0;for(int i=0;i<count;i++){await Task.Yield();sum+=i;}return sum;}
    public static async Task CancelAsync(CancellationToken token,bool ignore){await Task.Delay(1,ignore?CancellationToken.None:token);}
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<string> OuterAsync(){await Task.CompletedTask;return await InnerAsync();}
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<string> InnerAsync(){await Task.CompletedTask;return new StackTrace(true).ToString();}
}
