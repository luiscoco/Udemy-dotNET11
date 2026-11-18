using System.Net;
using System.Net.Http.Json;
namespace NativeCourse;
public sealed record ServerNote(int Version, string Title);
public sealed record Change(string OperationId, int ExpectedVersion, string Title);
public sealed class NoteSync(NoteRepository repository) : IDisposable
{
    private readonly HttpClient client = new() { BaseAddress = new Uri(Environment.GetEnvironmentVariable("COURSE_SYNC_URL") ?? "http://127.0.0.1:5145"), Timeout = TimeSpan.FromSeconds(10) };
    private readonly SemaphoreSlim gate = new(1);
    public async Task<string> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var note = repository.Read();
            if (note.OperationId is null) return "Already synchronized.";
            using var response = await client.PutAsJsonAsync("/notes/1", new Change(note.OperationId,note.Version,note.Title),cancellationToken);
            var server = (await response.Content.ReadFromJsonAsync<ServerNote>(cancellationToken))!;
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                repository.Conflict(server);
                return "Conflict detected. Your offline edit is retained. Choose a resolution.";
            }
            response.EnsureSuccessStatusCode();
            repository.Acknowledge(note.OperationId,server);
            return $"Synchronized at server version {server.Version}.";
        }
        finally { gate.Release(); }
    }
    public async Task<int> RepeatAsync(Change change)
    {
        using var response = await client.PutAsJsonAsync("/notes/1",change);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServerNote>())!.Version;
    }
    public void Dispose() { client.Dispose(); gate.Dispose(); }
}
