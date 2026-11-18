using Microsoft.Data.Sqlite;
namespace NativeCourse;
public sealed record LocalNote(string Title, int Version, string? OperationId, string? ConflictTitle, int? ConflictVersion);
public sealed class NoteRepository : IDisposable
{
    private readonly SqliteConnection database;
    public string FilePath { get; }
    public NoteRepository()
    {
        var testRoot = Environment.GetEnvironmentVariable("COURSE_ARTIFACTS");
        var directory = testRoot is null ? Path.Combine(FileSystem.AppDataDirectory, "course-145") : Path.Combine(testRoot, "sqlite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "notes.db");
        database = new SqliteConnection($"Data Source={FilePath}");
        database.Open();
        Execute("CREATE TABLE IF NOT EXISTS Notes(Id INTEGER PRIMARY KEY, Title TEXT NOT NULL, Version INTEGER NOT NULL, OperationId TEXT, ConflictTitle TEXT, ConflictVersion INTEGER)");
        Execute("INSERT OR IGNORE INTO Notes(Id,Title,Version) VALUES(1,'Original draft',1)");
    }
    public LocalNote Read()
    {
        using var command = database.CreateCommand();
        command.CommandText = "SELECT Title,Version,OperationId,ConflictTitle,ConflictVersion FROM Notes WHERE Id=1";
        using var reader = command.ExecuteReader();
        reader.Read();
        return new(reader.GetString(0),reader.GetInt32(1),reader.IsDBNull(2)?null:reader.GetString(2),reader.IsDBNull(3)?null:reader.GetString(3),reader.IsDBNull(4)?null:reader.GetInt32(4));
    }
    public void Edit(string title)
    {
        using var command = database.CreateCommand();
        command.CommandText = "UPDATE Notes SET Title=$title,OperationId=$operation,ConflictTitle=NULL,ConflictVersion=NULL WHERE Id=1";
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$operation", Guid.NewGuid().ToString("N"));
        command.ExecuteNonQuery();
    }
    public void Conflict(ServerNote server)
    {
        using var command = database.CreateCommand();
        command.CommandText = "UPDATE Notes SET ConflictTitle=$title,ConflictVersion=$version WHERE Id=1";
        command.Parameters.AddWithValue("$title",server.Title);
        command.Parameters.AddWithValue("$version",server.Version);
        command.ExecuteNonQuery();
    }
    public void AcceptServer()
    {
        using var transaction = database.BeginTransaction();
        using var command = database.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE Notes SET Title=ConflictTitle,Version=ConflictVersion,OperationId=NULL,ConflictTitle=NULL,ConflictVersion=NULL WHERE Id=1 AND ConflictVersion IS NOT NULL";
        command.ExecuteNonQuery();
        transaction.Commit();
    }
    public void Acknowledge(string operationId, ServerNote server)
    {
        using var command = database.CreateCommand();
        command.CommandText = "UPDATE Notes SET Version=$version,OperationId=NULL,ConflictTitle=NULL,ConflictVersion=NULL WHERE Id=1 AND OperationId=$operation";
        command.Parameters.AddWithValue("$version",server.Version);
        command.Parameters.AddWithValue("$operation",operationId);
        command.ExecuteNonQuery();
    }
    private void Execute(string sql)
    {
        using var command = database.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
    public void Dispose() => database.Dispose();
}
