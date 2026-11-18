using System.IO.Pipelines;
public static class Receiver
{
    public static async Task<string[]> ReadAsync(PipeReader reader)
    {
        var messages = new List<string>();
        try
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var remaining = result.Buffer;
                try
                {
                    while (Frames.TryRead(ref remaining, out var text)) messages.Add(text);
                    if (result.IsCompleted)
                    {
                        if (!remaining.IsEmpty) throw new InvalidDataException("Truncated frame at EOF");
                        return messages.ToArray();
                    }
                }
                finally { reader.AdvanceTo(remaining.Start, remaining.End); }
            }
        }
        finally { await reader.CompleteAsync(); }
    }
}
