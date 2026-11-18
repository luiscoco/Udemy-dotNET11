using System.Globalization;
using System.Text.Json;
public sealed class Invoice(int number, decimal net)
{
    public int Number { get; } = number > 0 ? number
        : throw new ArgumentOutOfRangeException(nameof(number));
    public decimal Net { get; } = net >= 0 ? net
        : throw new ArgumentOutOfRangeException(nameof(net));
    public string Customer
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    } = "Unassigned";
    public decimal Total => decimal.Round(Net * 1.20m, 2, MidpointRounding.AwayFromZero);
    public string ToJson() => $$"""
        {
          "number": {{Number}},
          "customer": {{JsonSerializer.Serialize(Customer)}},
          "total": {{Total.ToString(CultureInfo.InvariantCulture)}}
        }
        """;
}
