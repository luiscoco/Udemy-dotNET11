using System.Text.Json;
public union Reading(int, string);
public static class Readings
{
    public static async IAsyncEnumerable<Reading> Generate()
    {
        yield return new Reading(42);
        await Task.Yield();
        yield return new Reading("ready");
    }
    public static async Task<string> WriteAsync(Stream target, bool jsonLines)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsyncEnumerable(target, Generate(),
            topLevelValues: jsonLines, options: options);
        target.Position = 0;
        using var reader = new StreamReader(target, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
