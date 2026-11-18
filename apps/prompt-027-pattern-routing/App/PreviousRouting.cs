public static class PreviousRouting
{
    public static string Decide(Order? order)
    {
        if (order is null) return "Missing order";
        if (order.Net < 0) return "Reject amount";
        if (!order.Paid) return "Await payment";
        if (order.International && order.Net >= 1000) return "Customs review";
        if (order.Net >= 100) return "Priority";
        return "Standard";
    }
}
