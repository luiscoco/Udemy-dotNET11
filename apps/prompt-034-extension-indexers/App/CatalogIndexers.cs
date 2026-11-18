namespace CatalogIndexing;
public static class CatalogIndexers
{
    extension(InvoiceCatalog catalog)
    {
        public Invoice? this[string code] => catalog.Find(code);
    }
}
