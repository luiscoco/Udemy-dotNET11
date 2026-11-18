public static class Collections
{
    public static List<string> Names(string[] input) =>
        [with(capacity: input.Length * 2), .. input];

    public static HashSet<string> Codes() =>
        [with(StringComparer.OrdinalIgnoreCase), "PAID", "paid", "Paid"];

    public static HashSet<string> CaseSensitiveCodes() =>
        [with(StringComparer.Ordinal), "PAID", "paid"];
}
