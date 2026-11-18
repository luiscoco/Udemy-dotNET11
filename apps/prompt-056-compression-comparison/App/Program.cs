using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
byte[] json = JsonSerializer.SerializeToUtf8Bytes(Enumerable.Range(0, 4000)
    .Select(i => new { Id = i, Category = "course", Message = "Application event recorded" }));
byte[] noise = new byte[65536];
new Random(1234).NextBytes(noise);
if (args.FirstOrDefault() == "assume")
{
    byte[] encoded = CompressionLab.Compress(noise, "GZip");
    Console.WriteLine($"Seeded random payload {noise.Length}; GZip {encoded.Length}");
    Check.That(encoded.Length < noise.Length, "expected every payload to become smaller");
    return;
}
var report = new List<string> { "payload,codec,inputBytes,compressedBytes,ratio,encodeMs,decodeMs" };
foreach (var payload in new[] { (Name: "event-json", Data: json), (Name: "seeded-random", Data: noise) })
foreach (string codec in new[] { "Zstandard", "GZip", "Brotli" })
{
    _ = CompressionLab.Expand(CompressionLab.Compress(payload.Data, codec), codec);
    var watch = Stopwatch.StartNew();
    byte[] packed = [];
    for (int i = 0; i < 5; i++) packed = CompressionLab.Compress(payload.Data, codec);
    double encoding = watch.Elapsed.TotalMilliseconds / 5;
    watch.Restart();
    byte[] restored = [];
    for (int i = 0; i < 5; i++) restored = CompressionLab.Expand(packed, codec);
    double decoding = watch.Elapsed.TotalMilliseconds / 5;
    Check.That(restored.SequenceEqual(payload.Data), $"{payload.Name} / {codec} round-trip preserves every byte");
    string row = FormattableString.Invariant($"{payload.Name},{codec},{payload.Data.Length},{packed.Length},{(double)packed.Length / payload.Data.Length:F4},{encoding:F3},{decoding:F3}");
    report.Add(row);
    Console.WriteLine(row);
}
Directory.CreateDirectory("artifacts");
await File.WriteAllLinesAsync("artifacts/compression.csv", report);
Console.WriteLine("Measurements describe these payloads, levels and this machine; they are not a general benchmark.");
