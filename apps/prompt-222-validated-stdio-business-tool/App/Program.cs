using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
if(args.Contains("stdio"))
{
    var builder=McpSession.ServerBuilder();
    builder.Services.AddSingleton(new ReservationStore(!args.Contains("unsafe")));
    builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<ReservationTools>();
    await builder.Build().RunAsync();return;
}
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(45));
await using var client=await McpSession.Stdio([args.Contains("problem")?"unsafe":"safe"],deadline.Token);
var tool=(await client.ListToolsAsync()).Single(t=>t.Name=="reserve_stock");
Check.That(tool.JsonSchema.GetProperty("properties").TryGetProperty("quantity",out var parameter)&&parameter.GetProperty("type").GetString()=="integer",
    "the generated tool schema declares the quantity type");
var request=McpSession.Args(("sku","NET11"),("quantity",2),("requestId","reservation-7"));
var first=await client.CallToolAsync("reserve_stock",request,cancellationToken:deadline.Token);
var duplicate=await client.CallToolAsync("reserve_stock",request,cancellationToken:deadline.Token);
Check.That(first.IsError!=true&&McpSession.Text(first)==McpSession.Text(duplicate),"the real tool returns an idempotent local reservation receipt");
var changed=await client.CallToolAsync("reserve_stock",McpSession.Args(("sku","NET11"),("quantity",3),("requestId","reservation-7")),cancellationToken:deadline.Token);
Check.That(changed.IsError==true,"a receipt identifier cannot be reused for a changed quantity");
var invalid=await client.CallToolAsync("reserve_stock",McpSession.Args(("sku","NET11"),("quantity",0),("requestId","invalid-7")),cancellationToken:deadline.Token);
var snapshot=McpSession.Data(await client.CallToolAsync("stock_snapshot",cancellationToken:deadline.Token));
int effects=snapshot.GetProperty("effects").GetInt32(),stock=snapshot.GetProperty("stock").GetInt32();
Console.WriteLine($"MCP invalidIsError={invalid.IsError} mutations={effects} remaining={stock}");
Check.That(invalid.IsError==true&&effects==1&&stock==8,"domain validation rejects invalid data before mutation","MCP invalid reservation was accepted");
Check.That(McpSession.Text(invalid).Contains("Quantity must be 1 to 5"),"tool errors give a safe actionable business message");
McpSession.Save(new {passed=true,realStdio=true,invalidIsError=invalid.IsError,mutations=effects,remaining=stock,idempotent=McpSession.Text(first)==McpSession.Text(duplicate),changedPayloadRejected=changed.IsError==true});
