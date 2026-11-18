using ModelContextProtocol;
using System.Text.Json;
public sealed class ReservationStore(bool validate=true)
{
    private readonly object sync=new();private int stock=10,effects;
    private readonly Dictionary<string,string> receipts=[];
    public int Effects => effects;
    public string Snapshot() { lock(sync)return JsonSerializer.Serialize(new { stock,effects }); }
    public string Reserve(string sku,int quantity,string requestId)
    {
        lock(sync)
        {
            if(sku!="NET11"||!System.Text.RegularExpressions.Regex.IsMatch(requestId,"^[a-z0-9-]{1,32}$"))
                throw new McpException("Use the known SKU and a bounded request identifier.");
            if(validate&&(quantity is <1 or >5))throw new McpException("Quantity must be 1 to 5 and available.");
            if(receipts.TryGetValue(requestId,out var old))
            {
                if(JsonSerializer.Deserialize<JsonElement>(old).GetProperty("quantity").GetInt32()!=quantity)
                    throw new McpException("Request identifier already belongs to another quantity.");
                return old;
            }
            if(validate&&quantity>stock)throw new McpException("Quantity must be 1 to 5 and available.");
            stock-=quantity;effects++;
            return receipts[requestId]=JsonSerializer.Serialize(new { sku,quantity,remaining=stock,total=quantity*12.50m });
        }
    }
}
