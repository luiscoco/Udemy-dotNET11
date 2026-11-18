using System.Text;
using System.Text.Json;
Reading number = new(42);
Reading text = new("ready");
Check.That(JsonSerializer.Serialize(number) == "42", "integer union case writes a JSON number");
Check.That(JsonSerializer.Deserialize<Reading>(JsonSerializer.Serialize(text)).Value is string value && value == "ready",
    "string union case round-trips by token kind");
if (args.FirstOrDefault() == "unsupported")
{
    _ = JsonSerializer.Deserialize<Reading>("{\"value\":42}");
    return;
}
using var stream = new MemoryStream();
bool lines = args.FirstOrDefault() != "array";
string output = await Readings.WriteAsync(stream, lines);
Console.WriteLine(output);
Check.That(output == "42\n\"ready\"\n", "JSON Lines uses compact values and a final LF");
using var input = new MemoryStream(Encoding.UTF8.GetBytes(output));
var restored = new List<Reading>();
await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<Reading>(input, topLevelValues: true))
    restored.Add(item);
Check.That(restored.Count == 2 && restored[0].Value is int n && n == 42,
    "the streaming reader restores both top-level values");
