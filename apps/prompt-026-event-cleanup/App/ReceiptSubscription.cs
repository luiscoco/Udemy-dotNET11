public sealed class ReceiptSubscription : IDisposable
{
    private readonly InvoicePublisher publisher;
    private readonly EventHandler<InvoiceEventArgs> handler;
    private bool disposed;

    public ReceiptSubscription(InvoicePublisher publisher, EventHandler<InvoiceEventArgs> handler)
    {
        this.publisher = publisher;
        this.handler = handler;
        publisher.Completed += handler;
    }

    public void Dispose()
    {
        if (disposed)
            return;
        publisher.Completed -= handler;
        disposed = true;
    }
}
