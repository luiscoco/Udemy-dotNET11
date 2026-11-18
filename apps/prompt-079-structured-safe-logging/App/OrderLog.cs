using Microsoft.Extensions.Logging;
public static partial class OrderLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Order {OrderId} processed {ItemCount} items in {ElapsedMs} ms")]
    public static partial void Processed(ILogger logger, string orderId, int itemCount, long elapsedMs);
}
public sealed record IncomingOrder(string OrderId, string Credential, string CustomerEmail, string Body);
