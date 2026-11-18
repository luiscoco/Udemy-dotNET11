string? customer = null;
Console.WriteLine($"Optional customer length: {customer?.Length ?? 0}");
Console.WriteLine(InvoicePresenter.Show(154.68m));

public sealed class InvoicePresenter
{
    public static string Show(decimal total) => $"Receipt total: {total:0.00}";
}
