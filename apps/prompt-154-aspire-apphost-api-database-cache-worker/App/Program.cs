using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
await using var application = await builder.BuildAsync();
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
await application.StartAsync(deadline.Token);
await application.ResourceNotifications.WaitForResourceAsync("api", "Running", deadline.Token);
using var client = application.CreateHttpClient("api");

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

if (args.FirstOrDefault() == "missing")
    Check.That(body.Contains("Missing worker"), "incorrect expectation: the configured worker output is absent");

Directory.CreateDirectory("artifacts");
await File.WriteAllTextAsync("artifacts/aspire-result.json", JsonSerializer.Serialize(new {passed=true, lesson=154, evidence="Executed Aspire AppHost with PostgreSQL, Redis, API and worker", body}));
Console.WriteLine("service: actual AppHost resource graph, health and HTTP/database/cache round trips verified");
