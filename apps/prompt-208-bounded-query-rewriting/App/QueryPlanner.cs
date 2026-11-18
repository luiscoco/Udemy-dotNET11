public sealed record Rewrite(string First, string Second);
public static class QueryPlanner
{
    public static string[] Plan(string original, IEnumerable<string> generated, bool enforce = true)
    {
        var acceptable = generated.Where(q => !string.IsNullOrWhiteSpace(q) && q.Length <= 100);
        return acceptable.Prepend(original).Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(enforce ? 3 : 30).ToArray();
    }
    public static string[] Fuse(IEnumerable<IEnumerable<Hit>> lists) => lists
        .SelectMany(list => list.Select((hit,rank) => (hit.Id, Score: 1.0 / (60 + rank + 1))))
        .GroupBy(x => x.Id).OrderByDescending(g => g.Sum(x => x.Score)).ThenBy(g => g.Key)
        .Select(g => g.Key).ToArray();
}
