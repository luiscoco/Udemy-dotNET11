Course.Payments.Gateway.Charge(12m,"receipt-7");
namespace Course.Payments {public static class Gateway {public static void Charge(decimal amount,string idempotencyKey)
{if(string.IsNullOrWhiteSpace(idempotencyKey))throw new ArgumentException("A runtime key is required");Console.WriteLine("Synthetic payment "+idempotencyKey);}}}
