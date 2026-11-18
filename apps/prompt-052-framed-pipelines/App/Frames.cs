using System.Buffers;
using System.Buffers.Binary;
using System.Text;
public static class Frames
{
    public const int MaximumPayload = 1024;
    public static byte[] Encode(string text)
    {
        byte[] payload = Encoding.UTF8.GetBytes(text);
        if (payload.Length > MaximumPayload) throw new InvalidDataException("Frame exceeds limit");
        byte[] frame = new byte[4 + payload.Length];
        BinaryPrimitives.WriteInt32BigEndian(frame, payload.Length);
        payload.CopyTo(frame, 4);
        return frame;
    }
    public static bool TryRead(ref ReadOnlySequence<byte> buffer, out string text)
    {
        text = "";
        var reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadBigEndian(out int length)) return false;
        if (length < 0 || length > MaximumPayload) throw new InvalidDataException("Invalid frame length");
        if (reader.Remaining < length) return false;
        text = new UTF8Encoding(false, true).GetString(buffer.Slice(4, length).ToArray());
        buffer = buffer.Slice(4L + length);
        return true;
    }
}
