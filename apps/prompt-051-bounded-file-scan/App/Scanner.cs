using System.Security.Cryptography;
public sealed record ScanResult(long Bytes, long Lines, int LargestRead, string Hash);
public static class Scanner
{
    public const int BufferSize = 16 * 1024;
    public static async Task<ScanResult> ScanAsync(string path, CancellationToken token)
    {
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var buffer = new byte[BufferSize];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long bytes = 0, lines = 0;
        int largest = 0, count;
        while ((count = await file.ReadAsync(buffer.AsMemory(), token)) != 0)
        {
            hash.AppendData(buffer, 0, count);
            for (int i = 0; i < count; i++) if (buffer[i] == (byte)'\n') lines++;
            bytes += count;
            largest = Math.Max(largest, count);
        }
        return new(bytes, lines, largest, Convert.ToHexString(hash.GetHashAndReset()));
    }
}
