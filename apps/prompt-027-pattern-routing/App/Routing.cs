public sealed record Order(decimal Net, bool Paid, bool International);

public static class Routing
{
    public static string Decide(Order? order) => order switch
    {
        null => "Missing order",
        { Net: < 0 } => "Reject amount",
        { Paid: false } => "Await payment",
        { International: true, Net: >= 1000 } => "Customs review",
        { Net: >= 100 } => "Priority",
        _ => "Standard"
    };
}
