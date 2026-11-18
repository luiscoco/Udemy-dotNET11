public interface IPaymentGateway
{
    Task<bool> AuthorizeAsync(decimal amount);
}
public sealed class BookingService(IPaymentGateway gateway, int initialSeats)
{
    public int AvailableSeats { get; private set; } = initialSeats;
    public async Task<bool> BookAsync(int quantity)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > AvailableSeats) return false;
        bool approved = await gateway.AuthorizeAsync(quantity * 25m);
        if (!approved) return false;
        AvailableSeats -= quantity;
        return true;
    }
}
