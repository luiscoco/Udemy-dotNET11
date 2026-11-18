using System.Buffers;
public sealed class Segment : ReadOnlySequenceSegment<byte>
{
    public Segment(byte[] bytes) => Memory = bytes;
    public Segment Append(byte[] bytes)
    {
        var next = new Segment(bytes) { RunningIndex = RunningIndex + Memory.Length };
        Next = next;
        return next;
    }
    public static void Verify()
    {
        var first = new Segment(new byte[] { 1, 2 });
        var last = first.Append(new byte[] { 3, 4 });
        var sequence = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
        using var stream = new ReadOnlySequenceStream(sequence);
        Span<byte> destination = stackalloc byte[4];
        stream.ReadExactly(destination);
        Check.That(destination.SequenceEqual(new byte[] { 1, 2, 3, 4 }), "segmented input is read without flattening the source");
    }
}
