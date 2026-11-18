using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public static class SyncFixture
{
    public static void Map(WebApplication app)
    {
        var gate = new object();
        var current = new ServerNote(2, "Server revision");
        var completed = new Dictionary<string, ServerNote>();
        int mutations = 0;
        app.MapPut("/notes/1", (Change change) =>
        {
            lock (gate)
            {
                if (completed.TryGetValue(change.OperationId, out var prior)) return Results.Ok(prior);
                if (change.ExpectedVersion != current.Version) return Results.Conflict(current);
                current = new ServerNote(current.Version + 1, change.Title);
                completed.Add(change.OperationId, current);
                mutations++;
                return Results.Ok(current);
            }
        });
        app.MapGet("/metrics", () => { lock (gate) return new { mutations, current.Version }; });
    }
    public sealed record Change(string OperationId, int ExpectedVersion, string Title);
    public sealed record ServerNote(int Version, string Title);
}
