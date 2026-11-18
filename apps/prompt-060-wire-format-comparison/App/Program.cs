using System.Text;
using System.Text.Json;
using MessagePack;
var value = new Reading(7, "classroom", 21500);
byte[] json = JsonSerializer.SerializeToUtf8Bytes(value);
byte[] cbor = CborContract.Write(value);
var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
byte[] packed = MessagePackSerializer.Serialize(value, options);
Console.WriteLine(Encoding.UTF8.GetString(json));
Console.WriteLine($"JSON {json.Length} bytes; CBOR {cbor.Length} bytes; MessagePack {packed.Length} bytes");
if (args.FirstOrDefault() == "trailing")
{
    _ = CborContract.Read(cbor.Concat(new byte[] { 0 }).ToArray());
    return;
}
if (args.FirstOrDefault() == "mix")
{
    _ = JsonSerializer.Deserialize<Reading>(packed);
    return;
}
Check.That(JsonSerializer.Deserialize<Reading>(json) == value, "JSON named properties preserve the telemetry record");
Check.That(CborContract.Read(cbor) == value, "CBOR's explicit three-field array preserves the telemetry record");
Check.That(MessagePackSerializer.Deserialize<Reading>(packed, options) == value,
    "MessagePack's integer-key contract preserves the telemetry record");
Check.That(cbor.Length < json.Length && packed.Length < json.Length, "the binary layouts are smaller for this named-property fixture");
Directory.CreateDirectory("artifacts");
await File.WriteAllTextAsync("artifacts/sizes.csv", $"format,bytes\nJSON,{json.Length}\nCBOR,{cbor.Length}\nMessagePack,{packed.Length}\n");
Console.WriteLine("Select a documented media type, framing rule and schema version. Size depends on this contract and payload.");
