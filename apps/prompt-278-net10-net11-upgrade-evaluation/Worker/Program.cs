using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

bool broken = args.Contains("problem");
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
#if NET11_0
if(broken)options.PropertyNameCaseInsensitive=false;
#endif
const string input = "{\"quantity\":3,\"unitPrice\":12.50}";
var samples = new List<double>();
decimal total = 0;
for (int sample = 0; sample < 6; sample++)
{
    var watch = Stopwatch.StartNew();
    for (int i = 0; i < 5000; i++)
    {
        var quote = JsonSerializer.Deserialize<Quote>(input, options)!;
        total = quote.Quantity * quote.UnitPrice;
    }

    watch.Stop();
    if (sample > 0)
        samples.Add(watch.Elapsed.TotalMilliseconds);
}

Console.WriteLine(JsonSerializer.Serialize(new { runtime = RuntimeInformation.FrameworkDescription, total, samples, medianMs = samples.Order().ElementAt(2), operations = 5000, caseInsensitive = options.PropertyNameCaseInsensitive }));
public sealed record Quote(int Quantity, decimal UnitPrice);
