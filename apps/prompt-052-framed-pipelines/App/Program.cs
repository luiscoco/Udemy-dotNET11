using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
string mode = args.FirstOrDefault() ?? "verify";
byte[] bytes = Frames.Encode("alpha").Concat(Frames.Encode("café")).ToArray();
if (mode == "oversize") { bytes = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(bytes, 1025); }
if (mode == "truncated") bytes = bytes[..^1];
if (mode == "verify")
{
    var incomplete = new ReadOnlySequence<byte>(bytes.AsMemory(0, 6));
    long before = incomplete.Length;
    Check.That(!Frames.TryRead(ref incomplete, out _) && incomplete.Length == before,
        "an incomplete payload consumes no bytes");
}
var pipe = new Pipe();
Task<string[]> receiving = Receiver.ReadAsync(pipe.Reader);
for (int offset = 0; offset < bytes.Length; offset += 3)
{
    await pipe.Writer.WriteAsync(bytes.AsMemory(offset, Math.Min(3, bytes.Length - offset)));
    await Task.Delay(1);
}
await pipe.Writer.CompleteAsync();
string[] messages = await receiving;
Check.That(messages.SequenceEqual(new[] { "alpha", "café" }), "fragmented frames decode in order with UTF-8 intact");
Console.WriteLine(string.Join(" | ", messages));
