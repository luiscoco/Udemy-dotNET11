[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RuleAttribute(string code, int maximum) : Attribute
{
    public static int Created { get; private set; }
    public string Code { get; } = Record(code);
    public int Maximum { get; } = maximum;
    private static string Record(string code)
    {
        Created++;
        return code;
    }
}

[Rule("invoice.create", 100)]
public sealed class InvoiceRequest;
