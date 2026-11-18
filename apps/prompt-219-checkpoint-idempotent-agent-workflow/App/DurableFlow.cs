using Microsoft.Data.Sqlite;
using System.Text.Json;
public sealed record Progress(int Version,string Owner,string JobId,string Body,bool Sent);
public static class DurableFlow
{
    public static async Task Save(string folder,Progress state)
    {
        string file=Path.Combine(folder,"checkpoint.json"),temp=file+".tmp";
        await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(state)); File.Move(temp,file,true);
    }
    public static async Task<Progress> Load(string folder)
    {
        var state=JsonSerializer.Deserialize<Progress>(await File.ReadAllTextAsync(Path.Combine(folder,"checkpoint.json")))!;
        if(state.Owner!="alice"||state.Version is <1 or >2) throw new InvalidDataException("Invalid checkpoint envelope");
        return state;
    }
    public static async Task Effect(string folder,string operationId,string body)
    {
        using var database=new SqliteConnection("Data Source="+Path.Combine(folder,"external-outbox.db"));
        await database.OpenAsync();
        var command=database.CreateCommand();
        command.CommandText="CREATE TABLE IF NOT EXISTS receipts(operation_id TEXT PRIMARY KEY,body TEXT NOT NULL);";
        await command.ExecuteNonQueryAsync();
        command.CommandText="INSERT INTO receipts(operation_id,body) VALUES($id,$body) ON CONFLICT(operation_id) DO NOTHING";
        command.Parameters.AddWithValue("$id",operationId); command.Parameters.AddWithValue("$body",body);
        await command.ExecuteNonQueryAsync();
        command.CommandText="SELECT body FROM receipts WHERE operation_id=$id";
        string stored=(string)(await command.ExecuteScalarAsync())!;
        if(stored!=body) throw new InvalidOperationException("Idempotency key reused with changed payload");
    }
    public static async Task<long> Count(string folder)
    {
        using var database=new SqliteConnection("Data Source="+Path.Combine(folder,"external-outbox.db"));
        await database.OpenAsync(); var command=database.CreateCommand();
        command.CommandText="SELECT COUNT(*) FROM receipts"; return (long)(await command.ExecuteScalarAsync())!;
    }
}
