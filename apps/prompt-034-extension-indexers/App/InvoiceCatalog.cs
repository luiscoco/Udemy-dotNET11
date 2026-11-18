public sealed record Invoice(string Code, decimal Net);
public sealed class InvoiceCatalog(Invoice[] invoices)
{
    public int Inspections { get; private set; }
    public Invoice? Find(string code)
    {
        Inspections = 0;
        foreach (var invoice in invoices)
        {
            Inspections++;
            if (invoice.Code == code) return invoice;
        }
        return null;
    }
}
