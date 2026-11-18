using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
public static class Routes
{
    public static void Map(WebApplication app)
    {
        var todos = app.MapGroup("/todos");
        todos.MapPost("/", (TodoInput input, TodoStore store, CancellationToken ct) =>
        { var item=store.Add(input,ct); return TypedResults.Created($"/todos/{item.Id}",item); });
        todos.MapGet("/{id:int}", Get);
        todos.MapPut("/{id:int}", Results<NoContent, NotFound> (int id, TodoInput input, TodoStore store, CancellationToken ct) =>
            store.Update(id,input,ct) ? TypedResults.NoContent() : TypedResults.NotFound());
        todos.MapDelete("/{id:int}", Results<NoContent, NotFound> (int id, TodoStore store, CancellationToken ct) =>
            store.Delete(id,ct) ? TypedResults.NoContent() : TypedResults.NotFound());
        todos.MapGet("/wait", async (CancellationProbe probe, CancellationToken ct) =>
        {
            probe.Entered.TrySetResult();
            try { await Task.Delay(Timeout.InfiniteTimeSpan, ct); }
            catch (OperationCanceledException) when(ct.IsCancellationRequested) { probe.Cancelled.TrySetResult(); throw; }
            return TypedResults.NoContent();
        });
    }
    private static Results<Ok<Todo>, NotFound> Get(int id, TodoStore store, CancellationToken ct) =>
        store.Get(id,ct) is { } item ? TypedResults.Ok(item) : TypedResults.NotFound();
}
