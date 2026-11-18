using System.Net;
await using var api = new Api();
await api.StartAsync();
using var client = new HttpClient();
if (args.FirstOrDefault() == "abort")
{
    using var cancellation = new CancellationTokenSource();
    Task<HttpResponseMessage> pending = client.GetAsync(api.Address + "/api/client", cancellation.Token);
    await api.PeerEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
    cancellation.Cancel();
    try { using var response = await pending; }
    catch (OperationCanceledException)
    {
        await api.PeerExited.Task.WaitAsync(TimeSpan.FromSeconds(10));
        foreach (string line in api.Trace) Console.WriteLine(line);
        throw;
    }
    return;
}
if (args.FirstOrDefault() == "ignored")
{
    using var response = await client.GetAsync(api.Address + "/api/ignored");
    foreach (string line in api.Trace) Console.WriteLine(line);
    Console.WriteLine($"Ignored deadline HTTP {(int)response.StatusCode}");
    Check.That(response.StatusCode == HttpStatusCode.RequestTimeout, "a cancelled API token must reach downstream operations");
    return;
}
using (var success = await client.GetAsync(api.Address + "/api/ok"))
    Check.That(success.IsSuccessStatusCode && (await success.Content.ReadAsStringAsync()).Contains("42:peer"),
        "the real SQLite query and loopback HTTP request complete successfully");
using (var database = await client.GetAsync(api.Address + "/api/database"))
    Check.That(database.StatusCode == HttpStatusCode.RequestTimeout, "the deadline cancels the controlled database-boundary wait");
using (var outbound = await client.GetAsync(api.Address + "/api/outbound"))
    Check.That(outbound.StatusCode == HttpStatusCode.RequestTimeout, "the same request budget cancels outbound HTTP");
await api.PeerExited.Task.WaitAsync(TimeSpan.FromSeconds(10));
foreach (string line in api.Trace) Console.WriteLine(line);
Check.That(api.Trace.Count(x => x.StartsWith("api: cancellation")) == 2, "both cancellation boundaries retain their diagnostic");
Check.That(api.Trace.Any(x => x == "database: cancellable=True"), "the database boundary receives a cancellable token");
Console.WriteLine("SQLite async APIs execute synchronously; the cancellable wait is an explicit fixture before the query.");
