using Microsoft.Data.Sqlite;

// Representative authored AI-style candidate, deliberately insecure for review.
public static class Candidate
{
    public static List<string> Search(Store store, string tenant, string title)
    {
        using var c = store.Open();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT Secret FROM Records WHERE Tenant='" + tenant + "' AND Title='" + title + "'";
        using var rows = q.ExecuteReader();
        var result = new List<string>();
        while (rows.Read())
            result.Add(rows.GetString(0));
        return result;
    }
}
