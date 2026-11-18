namespace InvoiceSuite.Core;

public sealed record InvoiceLine(string Description, int Quantity, decimal UnitPrice)
{
    public decimal Net => Quantity * UnitPrice;
}

public sealed class Invoice
{
    public string Customer { get; }
    public IReadOnlyList<InvoiceLine> Lines { get; }
    public decimal Net => Lines.Sum(line => line.Net);
    public decimal Tax => Math.Round(Net * 0.20m, 2, MidpointRounding.AwayFromZero);
    public decimal Total => Net + Tax;

    public Invoice(string customer, params InvoiceLine[] lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customer);
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Length == 0 || lines.Any(line =>
            line is null || line.Quantity <= 0 || line.UnitPrice < 0))
            throw new ArgumentException("Use at least one valid invoice line.", nameof(lines));
        Customer = customer;
        Lines = Array.AsReadOnly((InvoiceLine[])lines.Clone());
    }

    public static int CustomerNameLength(string? customer) => customer?.Length ?? 0;
}
