using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
[TestClass]
public sealed class AcceptanceTests
{
    [TestMethod]
    public async Task Concurrency_and_fault_acceptance_criteria_are_enforced()
    {
        using var gate = new WorkGate();
        await using var app = WorkHost.Create(gate);
        await app.StartAsync();
        using var client = new HttpClient {BaseAddress=new Uri(app.Urls.Single()),Timeout=TimeSpan.FromSeconds(15)};
        var latencies = new ConcurrentBag<double>();
        async Task<HttpResponseMessage> Measure()
        {
            var timer = Stopwatch.StartNew();
            var response = await client.GetAsync("/work");
            latencies.Add(timer.Elapsed.TotalMilliseconds);
            return response;
        }
        try
        {
            var accepted = new[]{Measure(),Measure()};
            await gate.BothEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var rejected = await Task.WhenAll(Enumerable.Range(0,8).Select(_ => Measure())).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(rejected.All(response => response.StatusCode == HttpStatusCode.ServiceUnavailable), "Every excess request must be rejected while both slots are held.");
            foreach (var response in rejected) response.Dispose();
            gate.Release();
            var completed = await Task.WhenAll(accepted);
            Assert.IsTrue(completed.All(response => response.IsSuccessStatusCode));
            foreach (var response in completed) response.Dispose();
            Assert.AreEqual(2,gate.Maximum,"The maximum accepted concurrency must be two.");
            gate.InjectFailure = true;
            using var fault = await client.GetAsync("/work");
            Assert.AreEqual(HttpStatusCode.ServiceUnavailable,fault.StatusCode,"Injected failure must remain observable as HTTP 503.");
            gate.InjectFailure = false;
            using var recovery = await client.GetAsync("/work");
            Assert.AreEqual(HttpStatusCode.OK,recovery.StatusCode);
            var ordered = latencies.Order().ToArray();
            double p95 = ordered[(int)Math.Ceiling(ordered.Length * 0.95) - 1];
            string artifacts = Path.GetFullPath("../../../../artifacts",AppContext.BaseDirectory);
            Directory.CreateDirectory(artifacts);
            await File.WriteAllTextAsync(Path.Combine(artifacts,"load-observations.json"),JsonSerializer.Serialize(new {passed=true,requests=10,accepted=2,rejected=8,maxInFlight=gate.Maximum,p95Milliseconds=p95,faultStatus=(int)fault.StatusCode,recoveryStatus=(int)recovery.StatusCode}));
        }
        finally { gate.Release(); await app.StopAsync(); }
    }
}
