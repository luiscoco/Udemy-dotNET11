using Microsoft.Data.Sqlite;

public sealed class Store
{
    public string FileName { get; }

    public Store(string name)
    {
        Directory.CreateDirectory("artifacts");
        FileName = Path.GetFullPath(Path.Combine("artifacts", name + "-" + Guid.NewGuid().ToString("N") + ".db"));
    }

    public Store(string file, bool existing)
    {
        FileName = file;
    }

    public SqliteConnection Open()
    {
        var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = FileName, DefaultTimeout = 30 }.ToString());
        c.Open();
        return c;
    }

    public int Execute(string sql, params (string Name, object Value)[] values)
    {
        using var c = Open();
        using var q = c.CreateCommand();
        q.CommandText = sql;
        foreach (var v in values)
            q.Parameters.AddWithValue(v.Name, v.Value);
        return q.ExecuteNonQuery();
    }

    public long Scalar(string sql, params (string Name, object Value)[] values)
    {
        using var c = Open();
        using var q = c.CreateCommand();
        q.CommandText = sql;
        foreach (var v in values)
            q.Parameters.AddWithValue(v.Name, v.Value);
        return Convert.ToInt64(q.ExecuteScalar());
    }
}
