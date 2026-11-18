using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
public static class MinimalRoutes
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/minimal/todos");
        group.MapPost("/", (TodoInput input, TodoStore store, CancellationToken ct) =>
        { var item=store.Add(input,ct); return TypedResults.Created($"/minimal/todos/{item.Id}",item); });
        group.MapGet("/{id:int}", Results<Ok<Todo>,NotFound> (int id,TodoStore store,CancellationToken ct) =>
            store.Get(id,ct) is { } item ? TypedResults.Ok(item) : TypedResults.NotFound());
        group.MapPut("/{id:int}", Results<NoContent,NotFound> (int id,TodoInput input,TodoStore store,CancellationToken ct) =>
            store.Update(id,input,ct) ? TypedResults.NoContent() : TypedResults.NotFound());
        group.MapDelete("/{id:int}", Results<NoContent,NotFound> (int id,TodoStore store,CancellationToken ct) =>
            store.Delete(id,ct) ? TypedResults.NoContent() : TypedResults.NotFound());
    }
}
