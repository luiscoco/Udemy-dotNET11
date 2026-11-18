using System.Text.RegularExpressions;
public static partial class InvoicePatterns
{
    [GeneratedRegex(@"\A[A-Z]{2}-[0-9]{4}\z", RegexOptions.CultureInvariant, 100)]
    public static partial Regex Code();

    [GeneratedRegex(@"\A[A-Z]{2}-\d{4}\z", RegexOptions.CultureInvariant, 100)]
    public static partial Regex UnicodeDigits();

    [GeneratedRegex(@"\A(a|aa)+\z", RegexOptions.CultureInvariant, 10)]
    public static partial Regex Risky();
}
