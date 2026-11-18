public sealed class ReviewedRepository(Store store)
{
    public async Task<List<string>> Search(string tenant, string title, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        using var c = store.Open();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT Secret FROM Records WHERE Tenant=$tenant AND Title=$title";
        q.Parameters.AddWithValue("$tenant", tenant);
        q.Parameters.AddWithValue("$title", title);
        using var rows = await q.ExecuteReaderAsync(token);
        var result = new List<string>();
        while (await rows.ReadAsync(token))
            result.Add(rows.GetString(0));
        return result;
    }
}
