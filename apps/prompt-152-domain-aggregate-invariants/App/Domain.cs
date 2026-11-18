public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency)
    {
        if (amount < 0 || currency != "EUR") throw new ArgumentException("Invalid course price");
        Amount = amount;
        Currency = currency;
    }
}
public sealed class Cohort
{
    private readonly HashSet<string> students = new(StringComparer.OrdinalIgnoreCase);
    public int Capacity { get; }
    public Money Price { get; }
    public IReadOnlyCollection<string> Students => students.ToArray();
    public Cohort(int capacity, Money price)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
        Price = price;
    }
    public void Enroll(string student)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(student);
        if (students.Contains(student)) throw new InvalidOperationException("Duplicate enrollment");
        if (students.Count >= Capacity) throw new InvalidOperationException("Cohort is full");
        students.Add(student);
    }
}
public sealed class ScholarshipPolicy
{
    public Money Quote(Cohort cohort, bool eligible)
        => new(cohort.Price.Amount * (eligible ? 0.5m : 1m), cohort.Price.Currency);
}
