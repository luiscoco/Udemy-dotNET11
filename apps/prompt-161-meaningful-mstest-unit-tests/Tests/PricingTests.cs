[TestClass]
public sealed class PricingTests
{
    [TestMethod]
    [DataRow(1, 10, 10)]
    [DataRow(2, 10, 20)]
    [DataRow(3, 10, 27)]
    [DataRow(4, 10, 36)]
    public void Bulk_discount_starts_at_three(int quantity, int price, int expected)
    {
        var policy = new PricingPolicy();
        decimal result = policy.Total(quantity, price);
        Assert.AreEqual((decimal)expected, result, "The discount boundary must include quantity three.");
    }
    [TestMethod]
    public void Zero_quantity_is_rejected()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PricingPolicy().Total(0, 10));
    [TestMethod]
    public void Negative_price_is_rejected()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PricingPolicy().Total(1, -1));
    [TestMethod]
    public void Monetary_result_rounds_to_cents()
        => Assert.AreEqual(2.70m, new PricingPolicy().Total(3, 0.999m));
}
