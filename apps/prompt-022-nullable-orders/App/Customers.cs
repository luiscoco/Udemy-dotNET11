public sealed record Customer(string Name, string? Phone);

public sealed class CustomerDirectory
{
    private readonly Dictionary<string, Customer> customers = new()
    {
        ["A-1"] = new Customer("Ada", null)
    };

    public Customer? Find(string id) => customers.GetValueOrDefault(id);
}

public static class CustomerDisplay
{
    public static string Describe(Customer? customer)
    {
        if (customer is null)
            return "Customer unavailable";

        string phone = customer.Phone ?? "Phone pending";
        return $"{customer.Name} / {phone}";
    }

    public static string RequireName(string? input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input.Trim();
    }
}
