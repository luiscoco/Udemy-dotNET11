using System.Text;

public sealed class ReceiptWriter : IDisposable, IAsyncDisposable
{
    private readonly FileStream stream;
    private readonly List<string> trace;
    private bool disposed;

    public ReceiptWriter(string path, List<string> trace)
    {
        this.trace = trace;
        stream = new FileStream(path, FileMode.Create, FileAccess.Write,
            FileShare.None, 4096, FileOptions.Asynchronous);
    }

    public ValueTask WriteAsync(string receipt)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return stream.WriteAsync(Encoding.UTF8.GetBytes(receipt));
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        stream.Dispose();
        trace.Add("sync closed");
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        await stream.DisposeAsync();
        trace.Add("async closed");
    }
}
