using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
int[] data=Enumerable.Range(0,100000).Select(i=>i%100).ToArray();
[MethodImpl(MethodImplOptions.NoInlining)]
static long Kernel(int[] data)
{
    long sum=0;
    foreach(int value in data) sum+=value*2+3;
    return sum;
}
for(int i=0;i<100;i++) Kernel(data);
var clock=Stopwatch.StartNew();
long checksum=0;
for(int i=0;i<100;i++) checksum+=Kernel(data);
clock.Stop();
var report=new RuntimeReport(RuntimeInformation.FrameworkDescription,RuntimeFeature.IsDynamicCodeSupported,RuntimeFeature.IsDynamicCodeCompiled,checksum,clock.Elapsed.TotalMilliseconds);
Console.WriteLine(JsonSerializer.Serialize(report,ReportJson.Default.RuntimeReport));
public sealed record RuntimeReport(string Runtime,bool DynamicCodeSupported,bool DynamicCodeCompiled,long Checksum,double Milliseconds);
[System.Text.Json.Serialization.JsonSerializable(typeof(RuntimeReport))]
internal partial class ReportJson:System.Text.Json.Serialization.JsonSerializerContext { }
