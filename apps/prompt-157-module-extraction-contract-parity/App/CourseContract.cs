using System.Net;
using System.Net.Http.Json;
public sealed record CourseDto(int Id, string Title);
public sealed record LookupResult(string Status, CourseDto? Course = null);
public interface ICourseLookup
{
    Task<LookupResult> FindAsync(int id, CancellationToken cancellationToken);
}
public sealed class LocalLookup : ICourseLookup
{
    public Task<LookupResult> FindAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(id == 11 ? new LookupResult("found",new(11,"Architecture")) : new LookupResult("missing"));
    }
}
public sealed class HttpLookup(HttpClient client) : ICourseLookup
{
    public async Task<LookupResult> FindAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync($"/courses/{id}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound) return new("missing");
            if (!response.IsSuccessStatusCode) return new("unavailable");
            return new("found", await response.Content.ReadFromJsonAsync<CourseDto>(cancellationToken));
        }
        catch (HttpRequestException) { return new("unavailable"); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return new("unavailable"); }
    }
}
