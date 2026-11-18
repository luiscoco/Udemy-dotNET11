using ModelContextProtocol.Server;
using System.ComponentModel;
[McpServerToolType]
public sealed class ReservationTools
{
    [McpServerTool(Name="reserve_stock"),Description("Reserve local synthetic stock. No real order or purchase.")]
    public static string Reserve([Description("Known SKU NET11")]string sku,
        [Description("Quantity from 1 to 5")]int quantity,[Description("Stable request identifier")]string requestId,
        ReservationStore store)=>store.Reserve(sku,quantity,requestId);
    [McpServerTool(Name="stock_snapshot"),Description("Read the local teaching stock and mutation count.")]
    public static string Snapshot(ReservationStore store)=>store.Snapshot();
}
