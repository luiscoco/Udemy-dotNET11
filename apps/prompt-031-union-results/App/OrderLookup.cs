public static class OrderLookup
{
    public static LookupOutcome Find(int number)
    {
        if (number <= 0) return new Rejected("number must be positive");
        if (number == 42) return new Found(42, 128.90m);
        return new Missing(number);
    }
}
