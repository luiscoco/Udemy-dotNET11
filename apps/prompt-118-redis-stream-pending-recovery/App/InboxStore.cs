using Microsoft.Data.Sqlite;
public sealed class InboxStore : IDisposable
{
    private readonly SqliteConnection connection;
    public InboxStore(string file)
    {
        connection=new SqliteConnection($"Data Source={file};Pooling=False");connection.Open();
        using var create=connection.CreateCommand();
        create.CommandText="CREATE TABLE IF NOT EXISTS Inbox (MessageId TEXT PRIMARY KEY); CREATE TABLE IF NOT EXISTS Effects (Id INTEGER PRIMARY KEY, Total INTEGER NOT NULL); INSERT OR IGNORE INTO Effects VALUES (1,0);";
        create.ExecuteNonQuery();
    }
    public bool Apply(string messageId,int amount)
    {
        using var transaction=connection.BeginTransaction();
        using var insert=connection.CreateCommand();insert.Transaction=transaction;
        insert.CommandText="INSERT OR IGNORE INTO Inbox (MessageId) VALUES (@id)";
        insert.Parameters.AddWithValue("@id",messageId);var isNew=insert.ExecuteNonQuery()==1;
        if(isNew)
        {
            using var update=connection.CreateCommand();update.Transaction=transaction;
            update.CommandText="UPDATE Effects SET Total=Total+@amount WHERE Id=1";
            update.Parameters.AddWithValue("@amount",amount);update.ExecuteNonQuery();
        }
        transaction.Commit();return isNew;
    }
    public int Total
    {get{using var command=connection.CreateCommand();command.CommandText="SELECT Total FROM Effects WHERE Id=1";return Convert.ToInt32(command.ExecuteScalar());}}
    public void Dispose()=>connection.Dispose();
}
