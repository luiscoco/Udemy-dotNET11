using System.Text.Json;
public sealed record SessionEnvelope(int Version,string Owner,string Model,JsonElement State);
public static class SessionFile
{
    public static async Task Save(string file,SessionEnvelope envelope)
    {
        string temp=file+".tmp";
        await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(envelope));
        File.Move(temp,file,true);
    }
    public static async Task<SessionEnvelope> Load(string file,string authenticatedOwner)
    {
        var saved=JsonSerializer.Deserialize<SessionEnvelope>(await File.ReadAllTextAsync(file))!;
        if (saved.Version!=1 || saved.Model!=AgentRuntime.Model) throw new InvalidDataException("Incompatible session envelope");
        if (saved.Owner!=authenticatedOwner) throw new UnauthorizedAccessException("Session owner mismatch");
        return saved;
    }
}
