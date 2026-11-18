using System.IO.Compression;
public static class CompressionLab
{
    public static Stream Wrap(Stream target, string codec, bool compress) => (codec, compress) switch
    {
        ("Zstandard", true) => new ZstandardStream(target, CompressionLevel.Optimal, leaveOpen: true),
        ("GZip", true) => new GZipStream(target, CompressionLevel.Optimal, leaveOpen: true),
        ("Brotli", true) => new BrotliStream(target, CompressionLevel.Optimal, leaveOpen: true),
        ("Zstandard", false) => new ZstandardStream(target, CompressionMode.Decompress, leaveOpen: true),
        ("GZip", false) => new GZipStream(target, CompressionMode.Decompress, leaveOpen: true),
        ("Brotli", false) => new BrotliStream(target, CompressionMode.Decompress, leaveOpen: true),
        _ => throw new ArgumentException("Unknown codec")
    };
    public static byte[] Compress(byte[] input, string codec)
    {
        using var output = new MemoryStream();
        using (var compressor = Wrap(output, codec, true)) compressor.Write(input);
        return output.ToArray();
    }
    public static byte[] Expand(byte[] input, string codec)
    {
        using var source = new MemoryStream(input);
        using var decoder = Wrap(source, codec, false);
        using var output = new MemoryStream();
        decoder.CopyTo(output);
        return output.ToArray();
    }
}
