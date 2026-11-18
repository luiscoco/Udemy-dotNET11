using System.Security.Cryptography;
using System.Text;

public sealed record Chunk(string Id, string Tenant, string Reader, string Content, string Hash);
public sealed class Documents(Store store)
{
    public void Ingest()
    {
        store.Execute("CREATE TABLE Chunks(Id TEXT PRIMARY KEY,Tenant TEXT,Reader TEXT,Content TEXT,Hash TEXT)");
        foreach (var row in new[]
        {
            ("guide", "alpha", "alice", "Returns are accepted within thirty days."),
            ("restricted", "alpha", "bob", "CONFIDENTIAL_BOB: internal refund exception is seven days."),
            ("other-tenant", "beta", "alice", "CONFIDENTIAL_BETA: another tenant policy.")
        }

        )
        {
            string file = Path.Combine("artifacts", row.Item1 + ".md");
            File.WriteAllText(file, row.Item4);
            string text = File.ReadAllText(file);
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
            store.Execute("INSERT INTO Chunks VALUES($id,$tenant,$reader,$content,$hash)", ("$id", row.Item1), ("$tenant", row.Item2), ("$reader", row.Item3), ("$content", text), ("$hash", hash));
        }
    }

    public List<Chunk> Retrieve(string tenant, string reader, bool broken)
    {
        using var c = store.Open();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT Id,Tenant,Reader,Content,Hash FROM Chunks" + (broken ? "" : " WHERE Tenant=$tenant AND Reader=$reader") + " ORDER BY Id LIMIT 4";
        if (!broken)
        {
            q.Parameters.AddWithValue("$tenant", tenant);
            q.Parameters.AddWithValue("$reader", reader);
        }

        using var rows = q.ExecuteReader();
        var result = new List<Chunk>();
        while (rows.Read())
            result.Add(new(rows.GetString(0), rows.GetString(1), rows.GetString(2), rows.GetString(3), rows.GetString(4)));
        return result;
    }
}
