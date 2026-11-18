using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
int length = args.FirstOrDefault() == "wrong-input" ? 999999 : 1000000;
int[] data=Enumerable.Range(0,length).Select(i=>i%100).ToArray();
[MethodImpl(MethodImplOptions.NoInlining)]
static long Kernel(int[] data)
{
    long total=0;
    foreach(int value in data) total += value * 2 + 3;
    return total;
}
for(int i=0;i<100;i++) Kernel(data);
var clock=Stopwatch.StartNew();
long checksum=0;
for(int i=0;i<10;i++) checksum+=Kernel(data);
clock.Stop();
Console.WriteLine(JsonSerializer.Serialize(new { length, checksum, milliseconds=clock.Elapsed.TotalMilliseconds, runtime=RuntimeInformation.FrameworkDescription, architecture=RuntimeInformation.ProcessArchitecture.ToString(), serverGc=System.Runtime.GCSettings.IsServerGC, warmup=100, iterations=10 }));
