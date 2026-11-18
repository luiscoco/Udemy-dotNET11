using System.Globalization;
using System.Text;
public static class TextRules
{
    public static decimal ReadProtocolAmount(string value) =>
        decimal.Parse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    public static bool SameName(string left, string right) =>
        string.Equals(left.Normalize(NormalizationForm.FormC), right.Normalize(NormalizationForm.FormC),
            StringComparison.Ordinal);
    public static bool SameCode(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
