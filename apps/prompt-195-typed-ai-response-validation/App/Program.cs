using Microsoft.Extensions.AI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using var client = AiRuntime.Create();
var result = await client.GetResponseAsync<OrderDraft>(
    "Return an order for product pencil with quantity 2. Use the requested JSON schema.",
    options: AiRuntime.Options(80), useJsonSchemaResponseFormat: true);
OrderDraft draft = result.Result;
Console.WriteLine("SCHEMA " + JsonSerializer.Serialize(draft));
Check.That(OrderValidator.IsValid(draft), "live typed response passes business validation");
var invalid = new OrderDraft("pencil", 99);
bool rejected = false;
try { OrderValidator.Accept(invalid, !args.Contains("problem")); }
catch (ValidationException) { rejected = true; }
Console.WriteLine($"SCHEMA fixture quantity=99 rejected={rejected}");
AiRuntime.Evidence("validation-boundary.json", new { rejected, invalid });
Check.That(rejected, "business validation rejects quantity 99", "AI invalid quantity accepted");
bool malformedRejected = false;
try { JsonSerializer.Deserialize<OrderDraft>("{not-json}"); }
catch (JsonException) { malformedRejected = true; }
Check.That(malformedRejected, "malformed JSON rejected separately from business rules");
AiRuntime.Evidence("ai-evidence.json", new { passed = true, liveDraft = draft, rejected, malformedRejected });
