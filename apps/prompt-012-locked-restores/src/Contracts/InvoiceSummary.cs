namespace Course.Invoice.Contracts;

public sealed record InvoiceSummary(string Customer, decimal Net, decimal Tax)
{
    public decimal Total => Net + Tax;
}
