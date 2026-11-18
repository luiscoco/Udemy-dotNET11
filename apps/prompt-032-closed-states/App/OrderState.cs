public closed record OrderState;
public sealed record Draft(int Number) : OrderState;
public sealed record Paid(int Number, decimal Total) : OrderState;
public sealed record Dispatched(int Number, string Tracking) : OrderState;
public sealed record Cancelled(int Number) : OrderState;

public static class StateView
{
    public static string Describe(OrderState? state) => state switch
    {
        Draft => "draft",
        Paid => "paid",
        Dispatched => "dispatched",
        Cancelled => "cancelled",
        null => "missing state"
    };
}
