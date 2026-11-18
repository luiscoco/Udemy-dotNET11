using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.Security.Claims;
using System.ComponentModel;
using System.Text.Json;
public sealed class StockLedger(bool bypassPermissions=false)
{
    public int Reads { get; private set; }
    public ReservationStore Store { get; } = new();
    public string Read(ClaimsPrincipal user,string sku)
    {
        if(user.Identity?.IsAuthenticated!=true||user.FindFirst("tenant")?.Value!="a"||sku!="NET11")
            throw new McpException("Stock is not available to this caller.");
        Reads++;int current=JsonSerializer.Deserialize<JsonElement>(Store.Snapshot()).GetProperty("stock").GetInt32();
        return JsonSerializer.Serialize(new { sku,available=current });
    }
    public string Reserve(ClaimsPrincipal user,int quantity,string requestId)
    {
        if(!bypassPermissions&&(!user.IsInRole("operator")||!user.HasClaim("scope","mcp.write")||user.FindFirst("tenant")?.Value!="a"))
            throw new McpException("Write permission is required.");
        return Store.Reserve("NET11",quantity,requestId);
    }
}
[McpServerToolType]
public sealed class StockTools
{
    [McpServerTool(Name="read_stock",ReadOnly=true),Authorize(Policy="mcp.access")]
    [Description("Read stock for the synthetic SKU. Never creates a purchase.")]
    public static string Read(string sku,ClaimsPrincipal user,StockLedger ledger)=>ledger.Read(user,sku);
    [McpServerTool(Name="reserve_stock"),Authorize(Policy="stock.write")]
    [Description("Reserve only the local teaching stock for an authenticated operator.")]
    public static string Reserve(int quantity,string requestId,ClaimsPrincipal user,StockLedger ledger)=>ledger.Reserve(user,quantity,requestId);
}
