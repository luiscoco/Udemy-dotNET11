using MessagePack;
[MessagePackObject]
public sealed record Reading(
    [property: Key(0)] int DeviceId,
    [property: Key(1)] string Label,
    [property: Key(2)] int MilliDegrees);
