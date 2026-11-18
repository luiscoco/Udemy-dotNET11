using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public sealed record ChangeRequest(string Customer, string Email, int Version, string Approval);
public sealed class SupportTools(Store store, bool broken)
{
    public void Initialize() => store.Execute("CREATE TABLE Customers(Id TEXT PRIMARY KEY,Tenant TEXT,Email TEXT,Version INTEGER); INSERT INTO Customers VALUES('customer-1','alpha','old@example.test',1); CREATE TABLE Approvals(Id TEXT PRIMARY KEY,Actor TEXT,Tenant TEXT,Customer TEXT,Email TEXT,Version INTEGER,Expires INTEGER,Consumed INTEGER); CREATE TABLE Audit(Actor TEXT,Customer TEXT,Email TEXT)");
    public void Map(WebApplication app)
    {
        app.MapGet("/tools/{name}", (string name, ClaimsPrincipal user) =>
        {
            if (name != "lookup_customer")
                return Results.BadRequest(new { error = "tool is not approved" });
            using var c = store.Open();
            using var q = c.CreateCommand();
            q.CommandText = "SELECT Email,Version FROM Customers WHERE Id='customer-1' AND Tenant=$tenant";
            q.Parameters.AddWithValue("$tenant", user.FindFirstValue("tenant"));
            using var row = q.ExecuteReader();
            return row.Read() ? Results.Ok(new { email = row.GetString(0), version = row.GetInt32(1) }) : Results.NotFound();
        }).RequireAuthorization();
        app.MapPost("/approve", (ChangeRequest change, ClaimsPrincipal user) =>
        {
            string approval = Guid.NewGuid().ToString("N");
            store.Execute("INSERT INTO Approvals VALUES($id,$actor,$tenant,$customer,$email,$version,$expiry,0)", ("$id", approval), ("$actor", user.FindFirstValue("sub")!), ("$tenant", user.FindFirstValue("tenant")!), ("$customer", change.Customer), ("$email", change.Email), ("$version", change.Version), ("$expiry", DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds()));
            return Results.Ok(new { approval });
        }).RequireAuthorization("write");
        app.MapPost("/tools/set_contact_email", (ChangeRequest change, ClaimsPrincipal user) =>
        {
            using var c = store.Open();
            using var tx = c.BeginTransaction();
            using var gate = c.CreateCommand();
            gate.Transaction = tx;
            gate.CommandText = "UPDATE Approvals SET Consumed=1 WHERE Id=$id AND Actor=$actor AND Tenant=$tenant AND Customer=$customer AND Email=$email AND Version=$version AND Consumed=0 AND Expires>$now";
            foreach (var p in new (string, object)[]
            {
                ("$id", change.Approval),
                ("$actor", user.FindFirstValue("sub")!),
                ("$tenant", user.FindFirstValue("tenant")!),
                ("$customer", change.Customer),
                ("$email", change.Email),
                ("$version", change.Version),
                ("$now", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            }

            )
                gate.Parameters.AddWithValue(p.Item1, p.Item2);
            if (!broken && gate.ExecuteNonQuery() != 1)
                return Results.StatusCode(403);
            using var update = c.CreateCommand();
            update.Transaction = tx;
            update.CommandText = "UPDATE Customers SET Email=$email,Version=Version+1 WHERE Id=$id AND Tenant=$tenant AND Version=$version";
            update.Parameters.AddWithValue("$email", change.Email);
            update.Parameters.AddWithValue("$id", change.Customer);
            update.Parameters.AddWithValue("$tenant", user.FindFirstValue("tenant"));
            update.Parameters.AddWithValue("$version", change.Version);
            if (update.ExecuteNonQuery() != 1)
                return Results.Conflict();
            using var audit = c.CreateCommand();
            audit.Transaction = tx;
            audit.CommandText = "INSERT INTO Audit VALUES($actor,$id,$email)";
            audit.Parameters.AddWithValue("$actor", user.FindFirstValue("sub"));
            audit.Parameters.AddWithValue("$id", change.Customer);
            audit.Parameters.AddWithValue("$email", change.Email);
            audit.ExecuteNonQuery();
            tx.Commit();
            return Results.Ok(new { version = change.Version + 1 });
        }).RequireAuthorization("write");
    }
}
