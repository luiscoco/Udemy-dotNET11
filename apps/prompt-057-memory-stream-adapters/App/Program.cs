byte[] payload = new byte[1024 * 1024];
payload[0] = 7;
if (args.FirstOrDefault() == "snapshot")
{
    using var view = new ReadOnlyMemoryStream(payload);
    payload[0] = 9;
    Console.WriteLine($"First byte after owner mutation {view.ReadByte()}");
    Check.That(payload[0] == 7, "expected wrapping to create an isolated snapshot");
    return;
}
if (args.FirstOrDefault() == "overflow")
{
    using var fixedBuffer = new WritableMemoryStream(new byte[4]);
    fixedBuffer.Write(new byte[5]);
    return;
}
_ = Adapters.Allocation(payload, true);
_ = Adapters.Allocation(payload, false);
long copied = Adapters.Allocation(payload, true);
long wrapped = Adapters.Allocation(payload, false);
Console.WriteLine($"20 iterations / copy path allocated {copied:N0} bytes; wrapper path allocated {wrapped:N0} bytes");
Check.That(copied - wrapped >= 20L * payload.Length, "copy path allocates at least twenty additional payload buffers");
Adapters.VerifyWritable();
Adapters.VerifyText();
Segment.Verify();
Console.WriteLine("The existing MemoryStream(byte[]) constructor can also wrap an array without copying.");
