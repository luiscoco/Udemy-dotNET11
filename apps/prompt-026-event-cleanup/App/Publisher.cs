public sealed class InvoiceEventArgs(decimal total) : EventArgs
{
    public decimal Total { get; } = total;
}

public sealed class InvoicePublisher
{
    public event EventHandler<InvoiceEventArgs>? Completed;
    public void Publish(decimal total) => Completed?.Invoke(this, new InvoiceEventArgs(total));
}

public delegate decimal TaxCalculation(decimal net);
