using Microsoft.Data.Sqlite;
public sealed class OutboxStore : IDisposable
{
    private readonly SqliteConnection connection;
    public OutboxStore(string file)
    {
        connection=new SqliteConnection($"Data Source={file};Pooling=False");connection.Open();
        using var create=connection.CreateCommand();
        create.CommandText="CREATE TABLE IF NOT EXISTS Orders (Id TEXT PRIMARY KEY, Amount INTEGER NOT NULL); CREATE TABLE IF NOT EXISTS Outbox (Id TEXT PRIMARY KEY, Amount INTEGER NOT NULL, Sent INTEGER NOT NULL DEFAULT 0);";
        create.ExecuteNonQuery();
    }
    public void CreateOrder(string id,int amount,bool fail=false)
    {
        using var transaction=connection.BeginTransaction();
        using var command=connection.CreateCommand();command.Transaction=transaction;
        command.CommandText="INSERT INTO Orders VALUES (@id,@amount); INSERT INTO Outbox(Id,Amount) VALUES (@id,@amount);";
        command.Parameters.AddWithValue("@id",id);command.Parameters.AddWithValue("@amount",amount);command.ExecuteNonQuery();
        if(fail)throw new InvalidOperationException("Simulated transaction interruption");
        transaction.Commit();
    }
    public List<(string Id,int Amount)> Pending()
    {
        using var command=connection.CreateCommand();command.CommandText="SELECT Id,Amount FROM Outbox WHERE Sent=0 ORDER BY Id";
        using var reader=command.ExecuteReader();List<(string,int)> rows=[];
        while(reader.Read())rows.Add((reader.GetString(0),reader.GetInt32(1)));return rows;
    }
    public void MarkSent(string id)
    {using var command=connection.CreateCommand();command.CommandText="UPDATE Outbox SET Sent=1 WHERE Id=@id";command.Parameters.AddWithValue("@id",id);command.ExecuteNonQuery();}
    public int OrderCount
    {get{using var command=connection.CreateCommand();command.CommandText="SELECT COUNT(*) FROM Orders";return Convert.ToInt32(command.ExecuteScalar());}}
    public void Dispose()=>connection.Dispose();
}
