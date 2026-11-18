using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Diagnostics.Metrics;

public sealed class SaasApi(Store store, bool broken) : IDisposable
{
    private readonly Meter meter = new("Course.Saas");
    public long Allowed { get; private set; }
    public long Denied { get; private set; }

    public void Configure(WebApplicationBuilder b)
    {
        b.Services.AddDbContext<TenantDb>(o => o.UseSqlite("Data Source=" + store.FileName));
    }

    public async Task Map(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TenantDb>();
            await db.Database.EnsureCreatedAsync();
        }

        store.Execute("CREATE TABLE Usage(Tenant TEXT PRIMARY KEY,Used INTEGER); INSERT INTO Usage VALUES('alpha',0),('beta',0); CREATE TABLE Audit(Tenant TEXT,Actor TEXT,Document TEXT)");
        var counter = meter.CreateCounter<long>("course.saas.requests");
        app.MapGet("/documents/{id}", async (string id, ClaimsPrincipal actor, TenantDb db) =>
        {
            db.Tenant = actor.FindFirstValue("tenant")!;
            var query = broken ? db.Documents.IgnoreQueryFilters() : db.Documents;
            var doc = await query.SingleOrDefaultAsync(d => d.Id == id);
            return doc == null ? Results.NotFound() : Results.Ok(doc);
        }).RequireAuthorization();
        app.MapPost("/documents", (NewDocument document, ClaimsPrincipal actor) =>
        {
            string tenant = actor.FindFirstValue("tenant")!, subject = actor.FindFirstValue("sub")!;
            using var c = store.Open();
            using var tx = c.BeginTransaction();
            using var quota = c.CreateCommand();
            quota.Transaction = tx;
            quota.CommandText = "UPDATE Usage SET Used=Used+1 WHERE Tenant=$tenant AND Used<2";
            quota.Parameters.AddWithValue("$tenant", tenant);
            if (quota.ExecuteNonQuery() != 1)
            {
                Denied++;
                counter.Add(1, new KeyValuePair<string, object?>("outcome", "quota"));
                return Results.StatusCode(429);
            }

            using var insert = c.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText = "INSERT INTO Documents VALUES($id,$tenant,$content); INSERT INTO Audit VALUES($tenant,$actor,$id)";
            insert.Parameters.AddWithValue("$id", document.Id);
            insert.Parameters.AddWithValue("$tenant", tenant);
            insert.Parameters.AddWithValue("$content", document.Content);
            insert.Parameters.AddWithValue("$actor", subject);
            try
            {
                insert.ExecuteNonQuery();
                tx.Commit();
                Allowed++;
                counter.Add(1, new KeyValuePair<string, object?>("outcome", "accepted"));
                return Results.Created("/documents/" + document.Id, document);
            }
            catch (Microsoft.Data.Sqlite.SqliteException e)when (e.SqliteErrorCode == 19)
            {
                return Results.Conflict();
            }
        }).RequireAuthorization("write");
        app.MapGet("/audit", (ClaimsPrincipal actor) =>
        {
            using var c = store.Open();
            using var q = c.CreateCommand();
            q.CommandText = "SELECT Document FROM Audit WHERE Tenant=$tenant";
            q.Parameters.AddWithValue("$tenant", actor.FindFirstValue("tenant"));
            using var rows = q.ExecuteReader();
            var ids = new List<string>();
            while (rows.Read())
                ids.Add(rows.GetString(0));
            return Results.Ok(ids);
        }).RequireAuthorization();
    }

    public void Dispose() => meter.Dispose();
}
