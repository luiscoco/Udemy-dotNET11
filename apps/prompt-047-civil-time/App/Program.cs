var gap = Schedule.WallTime(new DateOnly(2026, 3, 29), new TimeOnly(2, 30));
if (args.FirstOrDefault() == "gap")
{
    Console.WriteLine(TimeZoneInfo.ConvertTimeToUtc(gap, Schedule.Zone));
    return;
}
var wall = Schedule.WallTime(new DateOnly(2026, 7, 1), new TimeOnly(12, 30));
DateTime utc = TimeZoneInfo.ConvertTimeToUtc(wall, Schedule.Zone);
Check.That(utc.Hour == 10 && utc.Kind == DateTimeKind.Utc, "summer wall time maps to 10:30 UTC");
var instant = new DateTimeOffset(utc);
Check.That(instant.ToLocalTime().ToUniversalTime() == instant, "machine-local display preserves the instant");
Check.That(Schedule.Zone.IsInvalidTime(gap), "spring gap contains no local instant");
var repeated = Schedule.WallTime(new DateOnly(2026, 10, 25), new TimeOnly(2, 30));
DateTimeOffset[] candidates = Schedule.Candidates(repeated);
Check.That(candidates.Length == 2 && candidates[1] - candidates[0] == TimeSpan.FromHours(1),
    "autumn repeated time maps to two instants one hour apart");
Console.WriteLine($"Machine display zone {TimeZoneInfo.Local.Id}, summer UTC {instant:O}");
foreach (var candidate in candidates) Console.WriteLine($"Repeated-time candidate {candidate:O}");
