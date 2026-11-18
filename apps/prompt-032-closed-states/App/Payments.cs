public static class Payments
{
    public static Paid Pay(OrderState state, decimal total)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(total);
        return state switch
        {
            Draft draft => new Paid(draft.Number, total),
            _ => throw new InvalidOperationException("Only a draft can be paid")
        };
    }
}
