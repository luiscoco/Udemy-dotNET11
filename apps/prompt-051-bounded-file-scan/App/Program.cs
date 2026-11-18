using System.Security.Cryptography;
Directory.CreateDirectory("artifacts");
string path = "artifacts/events.log";
const int rows = 2_000_000;
if (!File.Exists(path) || new FileInfo(path).Length != rows * 9L)
{
    await using var writer = new StreamWriter(path);
    for (int i = 0; i < rows; i++) await writer.WriteAsync("event,42\n");
}
if (args.FirstOrDefault() == "materialize")
{
    var all = await File.ReadAllBytesAsync(path);
    Console.WriteLine($"Materialized byte array: {all.Length:N0} bytes");
    Check.That(all.Length <= Scanner.BufferSize, "expected a bounded input buffer");
    return;
}
using var cancellation = new CancellationTokenSource();
if (args.FirstOrDefault() == "cancel") cancellation.Cancel();
var result = await Scanner.ScanAsync(path, cancellation.Token);
Check.That(result.Bytes == rows * 9L && result.Lines == rows, "all bytes and newline-terminated records are counted");
Check.That(result.LargestRead <= Scanner.BufferSize, "each requested input chunk stays within 16 KiB");
await using var verification = File.OpenRead(path);
string expected = Convert.ToHexString(await SHA256.HashDataAsync(verification));
Check.That(result.Hash == expected, "incremental digest matches an independent streaming digest");
Console.WriteLine($"File {result.Bytes:N0} bytes; lines {result.Lines:N0}; largest read {result.LargestRead}");
Console.WriteLine($"SHA256 {result.Hash}");
