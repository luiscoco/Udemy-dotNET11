using System.Formats.Cbor;
public static class CborContract
{
    public static byte[] Write(Reading value)
    {
        var writer = new CborWriter(CborConformanceMode.Canonical);
        writer.WriteStartArray(3);
        writer.WriteInt32(value.DeviceId);
        writer.WriteTextString(value.Label);
        writer.WriteInt32(value.MilliDegrees);
        writer.WriteEndArray();
        return writer.Encode();
    }
    public static Reading Read(byte[] bytes)
    {
        var reader = new CborReader(bytes, CborConformanceMode.Canonical);
        if (reader.ReadStartArray() != 3) throw new InvalidDataException("Expected three fields");
        var value = new Reading(reader.ReadInt32(), reader.ReadTextString(), reader.ReadInt32());
        reader.ReadEndArray();
        if (reader.BytesRemaining != 0) throw new InvalidDataException("Trailing CBOR content rejected");
        return value;
    }
}
