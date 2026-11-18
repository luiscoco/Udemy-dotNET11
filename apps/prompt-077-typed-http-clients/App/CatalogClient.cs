using System.Net.Http.Json;
public sealed record CatalogItem(int Id, string Name);
public sealed class CatalogClient(HttpClient http)
{
    public async Task<CatalogItem> ReadAsync(int id, CancellationToken token = default)
    {
        using var response = await http.GetAsync($"items/{id}", HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CatalogItem>(cancellationToken: token)
            ?? throw new InvalidDataException("Missing catalog response");
    }
    public Task<string> SlowAsync(CancellationToken token = default) => http.GetStringAsync("slow", token);
}
