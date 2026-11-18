using System.Collections.Immutable;

public sealed record InvoiceLine(string Description, int Quantity, decimal Price)
{
    public decimal Net => Quantity * Price;
}

public sealed record InvoiceSnapshot
{
    public string Customer { get; }
    public ImmutableArray<InvoiceLine> Lines { get; }
    public decimal Total => decimal.Round(Lines.Sum(line => line.Net) * 1.20m, 2);

    public InvoiceSnapshot(string customer, IEnumerable<InvoiceLine> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customer);
        ArgumentNullException.ThrowIfNull(lines);
        Customer = customer.Trim();
        Lines = lines.ToImmutableArray();
        if (Lines.IsEmpty || Lines.Any(line => line is null || line.Quantity <= 0 || line.Price < 0))
            throw new ArgumentException("Invoice lines must be valid", nameof(lines));
    }

    public InvoiceSnapshot Add(InvoiceLine line) => new(Customer, Lines.Add(line));
}
