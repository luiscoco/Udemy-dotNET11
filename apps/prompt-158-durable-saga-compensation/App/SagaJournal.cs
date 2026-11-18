using Microsoft.Data.Sqlite;
public sealed class SagaJournal : IDisposable
{
    private readonly SqliteConnection connection;
    public SagaJournal(string file)
    {
        connection = new SqliteConnection($"Data Source={file}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS Saga(Id TEXT PRIMARY KEY,State TEXT NOT NULL); INSERT OR IGNORE INTO Saga VALUES('enroll-11','Started');";
        command.ExecuteNonQuery();
    }
    public string State
    {
        get
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT State FROM Saga WHERE Id='enroll-11'";
            return (string)command.ExecuteScalar()!;
        }
    }
    public void Move(string expected, string next)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Saga SET State=$next WHERE Id='enroll-11' AND State=$expected";
        command.Parameters.AddWithValue("$next", next);
        command.Parameters.AddWithValue("$expected", expected);
        if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Concurrent saga transition");
    }
    public void Dispose() => connection.Dispose();
}
