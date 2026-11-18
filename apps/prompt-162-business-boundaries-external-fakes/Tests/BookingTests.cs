[TestClass]
public sealed class BookingTests
{
    [TestMethod]
    public async Task Exact_capacity_is_a_valid_booking()
    {
        var gateway = new PaymentFake(true);
        var service = new BookingService(gateway, 2);
        Assert.IsTrue(await service.BookAsync(2), "A booking equal to available capacity is valid.");
        Assert.AreEqual(0, service.AvailableSeats);
        CollectionAssert.AreEqual(new[]{50m}, gateway.Amounts.ToArray());
    }
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task Invalid_quantity_never_calls_the_external_gateway(int quantity)
    {
        var gateway = new PaymentFake(true);
        var service = new BookingService(gateway, 2);
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => service.BookAsync(quantity));
        Assert.AreEqual(0, gateway.Amounts.Count);
        Assert.AreEqual(2, service.AvailableSeats);
    }
    [TestMethod]
    public async Task Over_capacity_is_rejected_before_payment()
    {
        var gateway = new PaymentFake(true);
        var service = new BookingService(gateway, 2);
        Assert.IsFalse(await service.BookAsync(3));
        Assert.AreEqual(0, gateway.Amounts.Count);
    }
    [TestMethod]
    public async Task Declined_payment_preserves_capacity()
    {
        var gateway = new PaymentFake(false);
        var service = new BookingService(gateway, 2);
        Assert.IsFalse(await service.BookAsync(1));
        Assert.AreEqual(2, service.AvailableSeats);
        Assert.AreEqual(1, gateway.Amounts.Count);
    }
}
public sealed class PaymentFake(bool approved) : IPaymentGateway
{
    public List<decimal> Amounts { get; } = [];
    public Task<bool> AuthorizeAsync(decimal amount)
    {
        Amounts.Add(amount);
        return Task.FromResult(approved);
    }
}
