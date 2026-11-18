using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
await using var api = await ApiHarness.StartAsync(b => { b.Services.AddSingleton<TodoStore>(); b.Services.AddControllers(); },
    app => { app.MapControllers(); MinimalRoutes.Map(app); });
foreach(string style in new[] { "controllers", "minimal" })
{
    string route = $"/{style}/todos";
    using var created = await api.Client.PostAsJsonAsync(route + "/", new TodoInput("Shared contract"));
    var item = (await created.Content.ReadFromJsonAsync<Todo>())!;
    Check.That((int)created.StatusCode == 201 && created.Headers.Location is not null, style + " creation returns 201 with a resource location");
    Check.That((await api.Client.GetFromJsonAsync<Todo>(route + "/" + item.Id))!.Title == "Shared contract", style + " reads the same DTO contract");
    using var changed = await api.Client.PutAsJsonAsync(route + "/" + item.Id,new TodoInput("Changed"));
    using var deleted = await api.Client.DeleteAsync(route + "/" + item.Id);
    using var missing = await api.Client.GetAsync(route + "/" + item.Id);
    Console.WriteLine($"api: {style}: create {(int)created.StatusCode}; update {(int)changed.StatusCode}; delete {(int)deleted.StatusCode}; missing {(int)missing.StatusCode}");
    if(args.FirstOrDefault() == "missing") Check.That((int)missing.StatusCode == 200, "both endpoint styles must preserve the missing-resource contract");
    Check.That((int)changed.StatusCode == 204 && (int)deleted.StatusCode == 204 && (int)missing.StatusCode == 404, style + " retains update, delete and missing statuses");
}
