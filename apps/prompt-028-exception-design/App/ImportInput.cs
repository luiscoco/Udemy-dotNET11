using System.Globalization;

public static class ImportInput
{
    public static bool TryReadNet(string? input, out decimal net)
        => decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out net)
           && net >= 0;
}
