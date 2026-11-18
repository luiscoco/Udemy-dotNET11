using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
public sealed class RequestService(HttpClient peer, ConcurrentQueue<string> trace)
{
    public async Task<string> ExecuteAsync(string mode, string address, CancellationToken token)
    {
        trace.Enqueue($"service: cancellable={token.CanBeCanceled}");
        int number = await ReadDatabaseAsync(mode == "database", token);
        trace.Enqueue("service: database complete; starting HTTP");
        string path = mode is "outbound" or "client" ? "/peer/slow" : "/peer/ok";
        string response = await peer.GetStringAsync(address + path, token);
        return $"{number}:{response}";
    }
    private async Task<int> ReadDatabaseAsync(bool waitAtBoundary, CancellationToken token)
    {
        trace.Enqueue($"database: cancellable={token.CanBeCanceled}");
        if (waitAtBoundary) await Task.Delay(10_000, token); // Controlled wait before the SQLite call.
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(token);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 42";
        token.ThrowIfCancellationRequested();
        return Convert.ToInt32(await command.ExecuteScalarAsync(token));
    }
}
