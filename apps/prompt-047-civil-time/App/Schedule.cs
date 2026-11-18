public static class Schedule
{
    public static TimeZoneInfo Zone => TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
    public static DateTime WallTime(DateOnly date, TimeOnly time) =>
        date.ToDateTime(time, DateTimeKind.Unspecified);

    public static DateTimeOffset[] Candidates(DateTime wall)
    {
        if (Zone.IsInvalidTime(wall)) throw new ArgumentException("Local time is in the daylight-saving gap");
        TimeSpan[] offsets = Zone.IsAmbiguousTime(wall)
            ? Zone.GetAmbiguousTimeOffsets(wall) : [Zone.GetUtcOffset(wall)];
        return offsets.Select(offset => new DateTimeOffset(wall, offset).ToUniversalTime())
            .OrderBy(value => value).ToArray();
    }
}
