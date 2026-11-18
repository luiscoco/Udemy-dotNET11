using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
await using var application = await builder.BuildAsync();
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
var starting = application.StartAsync(deadline.Token);
try
{
await application.ResourceNotifications.WaitForResourceAsync("api", "Running", deadline.Token);
var apiResource = builder.Resources.OfType<ProjectResource>().Single(resource => resource.Name == "api");
var endpoint = new EndpointReference(apiResource, "http");
using var client = new HttpClient { BaseAddress = new Uri(endpoint.Url), Timeout = TimeSpan.FromSeconds(15) };
while (true)
{
    try
    {
        using var alive = await client.GetAsync("/alive", deadline.Token);
        if (alive.IsSuccessStatusCode) break;
    }
    catch (HttpRequestException) { }
    catch (OperationCanceledException) when (!deadline.IsCancellationRequested) { }
    await Task.Delay(100, deadline.Token);
}
var blocked = await client.GetAsync("/health", deadline.Token);
Check.That(blocked.StatusCode == HttpStatusCode.ServiceUnavailable, "readiness starts unhealthy while liveness is available");
Check.That((await client.GetAsync("/alive", deadline.Token)).IsSuccessStatusCode, "the running API has a separate liveness endpoint");
if (args.FirstOrDefault() == "premature")
    Check.That(blocked.IsSuccessStatusCode, "incorrect expectation: a running resource is already ready");
await client.PostAsync("/ready", null, deadline.Token);
await starting;

await application.ResourceNotifications.WaitForResourceHealthyAsync("api", deadline.Token);
string body = "";
while (!deadline.IsCancellationRequested)
{
    using var response = await client.GetAsync("/course", deadline.Token);
    body = await response.Content.ReadAsStringAsync(deadline.Token);
    if (response.IsSuccessStatusCode && body.Contains("Background worker completed")) break;
    await Task.Delay(200, deadline.Token);
}
Check.That(body.Contains("Background worker completed"), "the orchestrated worker wrote PostgreSQL and Redis and the API read both");
Check.That((await client.GetAsync("/health", deadline.Token)).IsSuccessStatusCode, "Aspire integration health checks report the dependencies healthy");


Directory.CreateDirectory("artifacts");
await File.WriteAllTextAsync("artifacts/aspire-result.json", JsonSerializer.Serialize(new {passed=true, lesson=156, evidence="Executed Aspire AppHost with PostgreSQL, Redis, API and worker", body}));
Console.WriteLine("service: actual AppHost resource graph, health and HTTP/database/cache round trips verified");

}
finally
{
    if (!starting.IsCompleted) deadline.Cancel();
    try { await starting; }
    catch (OperationCanceledException) when (deadline.IsCancellationRequested) { }
}
