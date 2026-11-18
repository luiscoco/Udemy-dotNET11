public sealed record Todo(int Id, string Title);
public sealed record TodoInput(string Title);
public sealed class TodoStore
{
    private readonly Dictionary<int, Todo> rows = []; private readonly object gate = new(); private int next;
    public Todo Add(TodoInput input, CancellationToken token) { token.ThrowIfCancellationRequested(); lock(gate) { var item = new Todo(++next, input.Title); rows.Add(item.Id, item); return item; } }
    public Todo? Get(int id, CancellationToken token) { token.ThrowIfCancellationRequested(); lock(gate) return rows.GetValueOrDefault(id); }
    public bool Update(int id, TodoInput input, CancellationToken token) { token.ThrowIfCancellationRequested(); lock(gate) { if(!rows.ContainsKey(id)) return false; rows[id] = new(id,input.Title); return true; } }
    public bool Delete(int id, CancellationToken token) { token.ThrowIfCancellationRequested(); lock(gate) return rows.Remove(id); }
}
public sealed class CancellationProbe
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
