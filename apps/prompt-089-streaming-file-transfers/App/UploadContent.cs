using System.Net;
public sealed class UploadContent(byte[] bytes,bool hold=false) : HttpContent
{
    protected override bool TryComputeLength(out long length) { length=0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream,TransportContext? context) => WriteAsync(stream,CancellationToken.None);
    protected override Task SerializeToStreamAsync(Stream stream,TransportContext? context,CancellationToken ct) => WriteAsync(stream,ct);
    private async Task WriteAsync(Stream stream,CancellationToken ct)
    {
        await stream.WriteAsync(bytes,ct); await stream.FlushAsync(ct);
        if(hold) await Task.Delay(Timeout.InfiniteTimeSpan,ct);
    }
}
