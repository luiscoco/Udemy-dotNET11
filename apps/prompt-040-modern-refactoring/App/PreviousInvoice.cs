using System.Text.Json;
public sealed class PreviousInvoice
{
    private readonly int number;
    private readonly decimal net;
    private string customer = "Unassigned";
    public PreviousInvoice(int number, decimal net)
    {
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number));
        if (net < 0) throw new ArgumentOutOfRangeException(nameof(net));
        this.number = number;
        this.net = net;
    }
    public int Number => number;
    public decimal Net => net;
    public string Customer
    {
        get => customer;
        set { ArgumentException.ThrowIfNullOrWhiteSpace(value); customer = value.Trim(); }
    }
    public decimal Total => decimal.Round(Net * 1.20m, 2, MidpointRounding.AwayFromZero);
    public string ToJson() => JsonSerializer.Serialize(new { number = Number, customer = Customer, total = Total });
}
