public sealed record Found(int Number, decimal Net);
public sealed record Missing(int Number);
public sealed record Rejected(string Reason);
public union LookupOutcome(Found, Missing, Rejected);

public static class OutcomeView
{
    public static string Describe(LookupOutcome outcome) => outcome switch
    {
        Found order => $"Order {order.Number}, net {order.Net:0.00}",
        Missing order => $"Missing order {order.Number}",
        Rejected error => $"Rejected {error.Reason}",
        null => throw new InvalidOperationException("Outcome has no value")
    };
}
