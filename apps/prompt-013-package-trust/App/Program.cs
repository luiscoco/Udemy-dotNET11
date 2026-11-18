using Newtonsoft.Json;

const string input = "{\"Customer\":\"Ada\",\"Net\":128.90,\"Tax\":25.78}";
var invoice = JsonConvert.DeserializeObject<Invoice>(input,
    new JsonSerializerSettings { MaxDepth = 32 })
    ?? throw new InvalidOperationException("Invoice missing");
if (invoice.Total != 154.68m) throw new InvalidOperationException("Wrong total");
Console.WriteLine(JsonConvert.SerializeObject(invoice));
Console.WriteLine("PASS: trusted exercise input totals EUR 154.68");

public sealed record Invoice(string Customer, decimal Net, decimal Tax)
{
    public decimal Total => Net + Tax;
}
