using Microsoft.Data.Sqlite;
public sealed class CourseStore : IDisposable
{
    private readonly SqliteConnection connection;
    public CourseStore(string file)
    {
        connection = new SqliteConnection($"Data Source={file}");
        connection.Open();
        Execute("CREATE TABLE IF NOT EXISTS Courses(Id INTEGER PRIMARY KEY,Title TEXT NOT NULL); CREATE TABLE IF NOT EXISTS Events(Id TEXT PRIMARY KEY,Title TEXT NOT NULL); CREATE TABLE IF NOT EXISTS CourseView(Id INTEGER PRIMARY KEY,Title TEXT NOT NULL); CREATE TABLE IF NOT EXISTS Applied(Id TEXT PRIMARY KEY);");
    }
    public void Rename(string commandId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        using var transaction = connection.BeginTransaction();
        using var exists = connection.CreateCommand();
        exists.Transaction = transaction;
        exists.CommandText = "SELECT COUNT(*) FROM Events WHERE Id=$id";
        exists.Parameters.AddWithValue("$id", commandId);
        if ((long)exists.ExecuteScalar()! == 0)
        {
            Execute("INSERT INTO Courses VALUES(11,$title) ON CONFLICT(Id) DO UPDATE SET Title=$title", transaction, title);
            using var append = connection.CreateCommand();
            append.Transaction = transaction;
            append.CommandText = "INSERT INTO Events VALUES($id,$title)";
            append.Parameters.AddWithValue("$id", commandId);
            append.Parameters.AddWithValue("$title", title);
            append.ExecuteNonQuery();
        }
        transaction.Commit();
    }
    public void Project()
    {
        using var transaction = connection.BeginTransaction();
        Execute("INSERT INTO CourseView SELECT 11,e.Title FROM Events e WHERE NOT EXISTS(SELECT 1 FROM Applied a WHERE a.Id=e.Id) ORDER BY e.rowid ON CONFLICT(Id) DO UPDATE SET Title=excluded.Title", transaction);
        Execute("INSERT OR IGNORE INTO Applied SELECT Id FROM Events", transaction);
        transaction.Commit();
    }
    public string? Query()
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Title FROM CourseView WHERE Id=11";
        return command.ExecuteScalar() as string;
    }
    private void Execute(string sql, SqliteTransaction? transaction = null, string? title = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        if (title is not null) command.Parameters.AddWithValue("$title", title);
        command.ExecuteNonQuery();
    }
    public void Dispose() => connection.Dispose();
}
