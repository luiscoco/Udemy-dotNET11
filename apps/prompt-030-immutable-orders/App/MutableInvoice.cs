public sealed class MutableInvoice(List<InvoiceLine> lines)
{
    public List<InvoiceLine> Lines { get; } = lines;
    public decimal Total => decimal.Round(Lines.Sum(line => line.Net) * 1.20m, 2);
}
