using ModelContextProtocol;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using System.Text.Json;
[McpServerToolType]
public sealed class QuoteTool
{
    [McpServerTool(Name="quote",ReadOnly=true),Authorize(Policy="mcp.access")]
    [Description("Calculate a synthetic catalogue quote. This does not place an order.")]
    public static string Quote(int quantity)
    {
        if(quantity is <1 or >10)throw new McpException("Quantity must be 1 to 10.");
        return JsonSerializer.Serialize(new { quantity,unitPrice=12.50m,total=quantity*12.50m,currency="EUR" });
    }
}
