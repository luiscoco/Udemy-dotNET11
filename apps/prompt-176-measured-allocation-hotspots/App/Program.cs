using System.Diagnostics;
using System.Text.Json;
const string input = "1,2,3,4,5,6,7,8,9,10";
const int iterations = 20000;
for (int i = 0; i < 1000; i++) { CourseParser.Allocating(input); CourseParser.Buffered(input); }
static (long Bytes, double Milliseconds, int Checksum) Measure(Func<CourseTotals> parse)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    var clock = Stopwatch.StartNew();
    int checksum = 0;
    for (int i = 0; i < iterations; i++) checksum += parse().Sum;
    clock.Stop();
    return (GC.GetAllocatedBytesForCurrentThread() - before, clock.Elapsed.TotalMilliseconds, checksum);
}
var baseline = Measure(() => CourseParser.Allocating(input));
var buffered = Measure(() => CourseParser.Buffered(input));
Check.That(baseline.Checksum == 55 * iterations && buffered.Checksum == baseline.Checksum, "both paths preserve the result");
Console.WriteLine($"Allocating bytes={baseline.Bytes}, milliseconds={baseline.Milliseconds:F2}");
Console.WriteLine($"Buffered bytes={buffered.Bytes}, milliseconds={buffered.Milliseconds:F2}");
Directory.CreateDirectory("artifacts");
File.WriteAllText("artifacts/allocation-measurements.json", JsonSerializer.Serialize(new { iterations, baseline = new { baseline.Bytes, baseline.Milliseconds, baseline.Checksum }, buffered = new { buffered.Bytes, buffered.Milliseconds, buffered.Checksum } }));
long selected = args.FirstOrDefault() == "problem" ? baseline.Bytes : buffered.Bytes;
Check.That(selected < baseline.Bytes / 4, "ALLOCATION_REGRESSION selected parser stays below a quarter of baseline allocation");
Check.That(CourseParser.Buffered("0,-1,12") == new CourseTotals(3,11), "value result preserves signed integer parsing");
