using System.Buffers;
using System.Text;
public static class Adapters
{
    public static long Allocation(byte[] payload, bool copy)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 20; i++)
        {
            using Stream stream = copy
                ? new MemoryStream(payload.ToArray(), writable: false)
                : new ReadOnlyMemoryStream(payload);
            _ = stream.ReadByte();
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
    public static void VerifyWritable()
    {
        byte[] target = new byte[4];
        using var writable = new WritableMemoryStream(target);
        writable.Write(new byte[] { 10, 20, 30, 40 });
        Check.That(target.SequenceEqual(new byte[] { 10, 20, 30, 40 }), "writes update the caller-owned fixed buffer");
    }
    public static void VerifyText()
    {
        using var text = new StringStream("café", Encoding.UTF8);
        using var reader = new StreamReader(text, Encoding.UTF8);
        Check.That(reader.ReadToEnd() == "café", "StringStream exposes encoded text through a stream API");
    }
}
