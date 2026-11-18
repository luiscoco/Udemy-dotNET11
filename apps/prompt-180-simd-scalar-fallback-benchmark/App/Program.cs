using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
bool problem = args.FirstOrDefault() == "problem";
Console.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}, hardware={Vector.IsHardwareAccelerated}, width={Vector<int>.Count}");
foreach (int length in new[]{0,1,Vector<int>.Count-1,Vector<int>.Count,Vector<int>.Count+1,100003})
{
    int[] values = Enumerable.Range(0,length).Select(i => i % 17 - 8).ToArray();
    int expected = NumericKernel.Scalar(values);
    Check.That(NumericKernel.Sum(values, forceScalar:true) == expected, $"forced scalar fallback length {length}");
    Check.That(NumericKernel.Sum(values, omitTail:problem) == expected, $"TAIL_REGRESSION vector and tail length {length}");
}
int[] input = Enumerable.Range(0,100003).Select(i => i % 17 - 8).ToArray();
for(int i=0;i<100;i++) { NumericKernel.Scalar(input); NumericKernel.Sum(input); }
static (double Milliseconds,int Checksum) Measure(int[] input, bool scalar)
{
    var clock = Stopwatch.StartNew();
    int checksum = 0;
    for(int i=0;i<1000;i++) checksum = unchecked(checksum + (scalar ? NumericKernel.Scalar(input) : NumericKernel.Sum(input)));
    return (clock.Elapsed.TotalMilliseconds,checksum);
}
var baseline = Measure(input,true);
var vector = Measure(input,false);
Check.That(baseline.Checksum == vector.Checksum, "measured paths preserve identical output");
Console.WriteLine($"Scalar={baseline.Milliseconds:F2} ms, SIMD={vector.Milliseconds:F2} ms, checksum={vector.Checksum}");
Directory.CreateDirectory("artifacts");
File.WriteAllText("artifacts/simd-measurements.json",JsonSerializer.Serialize(new { runtime=RuntimeInformation.FrameworkDescription, width=Vector<int>.Count, hardware=Vector.IsHardwareAccelerated, scalarMs=baseline.Milliseconds, vectorMs=vector.Milliseconds, checksum=vector.Checksum }));
