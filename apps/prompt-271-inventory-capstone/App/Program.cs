using System.Net.Http.Json;

bool broken = args.Contains("problem");
using var authority = new Authority();
if (args.Contains("--serve"))
{
    var server = await InventoryApi.Create(authority, "/data/inventory.db", false, true);
    await server.RunAsync();
    return;
}

var store = new Store("inventory");
await using var api = await ApiSession.Start(await InventoryApi.Create(authority, store.FileName, broken));
Check.That((int)(await api.Client.GetAsync("/inventory")).StatusCode == 401, "anonymous inventory request receives 401");
api.Token(authority.Issue(permission: "read"));
Check.That((int)(await api.Client.PostAsJsonAsync("/reserve", new Reservation(1, "read"))).StatusCode == 403, "read-only actor cannot reserve stock");
api.Token(authority.Issue(expired: true));
Check.That((int)(await api.Client.GetAsync("/inventory")).StatusCode == 401, "expired token is rejected");
api.Token(authority.Issue());
Check.That((int)(await api.Client.PostAsJsonAsync("/reserve", new Reservation(0, "invalid"))).StatusCode == 400, "actual minimal API validation rejects zero quantity");
var first = await api.Client.PostAsJsonAsync("/reserve", new Reservation(2, "order-1"));
Check.That(first.IsSuccessStatusCode, "valid reservation commits");
var replay = await api.Client.PostAsJsonAsync("/reserve", new Reservation(2, "order-1"));
Check.That(replay.IsSuccessStatusCode, "same request replays idempotently");
Check.That((int)(await api.Client.PostAsJsonAsync("/reserve", new Reservation(3, "order-1"))).StatusCode == 409, "conflicting replay is rejected");
var oversell = await api.Client.PostAsJsonAsync("/reserve", new Reservation(7, "order-2"));
var current = await api.Client.GetFromJsonAsync<Product>("/inventory");
Console.WriteLine($"INVENTORY oversell={(int)oversell.StatusCode} remaining={current!.Available}");
Check.That((int)oversell.StatusCode == 409 && current.Available == 6, "conditional update prevents negative stock", "CAPSTONE inventory oversell boundary failed");
await Deployment.Verify(authority);
Harness.Save("capstone-evidence.json", new { passed = true, efCore = true, authentication = true, validation = true, idempotency = true, stock = current.Available, container = true });
