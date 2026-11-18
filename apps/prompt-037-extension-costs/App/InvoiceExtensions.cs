public sealed record Line(int Quantity, decimal Price);
public static class InvoiceExtensions
{
    extension(IEnumerable<Line> lines)
    {
        public decimal NetTotal => lines.CalculateNetTotal();
        public decimal CalculateNetTotal()
        {
            ArgumentNullException.ThrowIfNull(lines);
            decimal total = 0;
            foreach (var line in lines)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(line.Quantity);
                ArgumentOutOfRangeException.ThrowIfNegative(line.Price);
                total += line.Quantity * line.Price;
            }
            return total;
        }
    }
}
