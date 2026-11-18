using System.Text.Json;
using Microsoft.Extensions.Logging;
var capture = new CaptureProvider();
using var factory = LoggerFactory.Create(builder => builder.ClearProviders().AddProvider(capture));
var logger = factory.CreateLogger("Course.OrderProcessing");
var request = new IncomingOrder("ORD-42", "DEMO-CREDENTIAL-DO-NOT-LOG", "student@example.invalid", "DEMO-PRIVATE-BODY");
using (logger.BeginScope(new Dictionary<string, object?> { ["OperationId"] = "batch-7" }))
{
    OrderLog.Processed(logger, request.OrderId, 3, 12);
    if (args.FirstOrDefault() == "payload") logger.LogInformation("Payload {Payload}", request);
}
string serialized = JsonSerializer.Serialize(capture.Events);
Check.That(!serialized.Contains(request.Credential) && !serialized.Contains(request.CustomerEmail) && !serialized.Contains(request.Body),
    "credentials, customer details and the private body are excluded from captured log state");
var entry = capture.Events.Single();
Check.That(entry.EventId == 1001 && entry.Fields["OrderId"]?.ToString() == "ORD-42" &&
    entry.Fields["ItemCount"] is int count && count == 3, "the generated event preserves its ID and typed named properties");
Check.That(entry.Fields.ContainsKey("{OriginalFormat}") && entry.Fields["Scope.OperationId"]?.ToString() == "batch-7",
    "the template and safe operation scope remain structured");
Console.WriteLine(JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("Generated logging avoids runtime template parsing; it does not automatically redact unsafe arguments.");
