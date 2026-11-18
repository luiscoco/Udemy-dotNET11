namespace TemplateApp;

public sealed record Invoice(string Customer, decimal Net)
{
    public decimal Total => Net * 1.20m;
}
