using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

public static class ApiAdditions
{
    public static async Task<object> Verify()
    {
        using var text = new StringStream("capability audit ✓", Encoding.UTF8);
        using var reader = new StreamReader(text, Encoding.UTF8);
        string decoded = await reader.ReadToEndAsync();
        Check.That(decoded == "capability audit ✓", "actual StringStream preserves UTF-8 text");
        ReadOnlyMemory<byte> payload = new byte[]
        {
            11,
            22,
            33
        };
        using var memory = new ReadOnlyMemoryStream(payload);
        byte[] copied = new byte[3];
        int read = await memory.ReadAsync(copied);
        Check.That(read == 3 && copied.SequenceEqual(payload.ToArray()) && !memory.CanWrite, "actual ReadOnlyMemoryStream exposes read-only bytes");
        var bits = new BitArray(new[] { true, false, true, true, false });
        int count = bits.PopCount();
        Check.That(count == 3, "actual BitArray.PopCount counts set bits");
        var child = await Process.RunAndCaptureTextAsync(Harness.Dotnet, new[] { Assembly.GetExecutingAssembly().Location, "audit-child" });
        Check.That(child.ExitStatus.ExitCode == 0 && child.StandardOutput.Trim() == "API-280", "actual new process helper captures the owned child output and exit status");
        return new
        {
            stringStream = true,
            readOnlyMemoryStream = true,
            popCount = count,
            processCapture = true,
            childOutput = child.StandardOutput.Trim(),
            apiScope = "four executed supplemental probes, not exhaustive library coverage"
        };
    }
}
