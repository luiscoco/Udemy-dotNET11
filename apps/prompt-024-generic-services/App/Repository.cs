public sealed class Repository<T> where T : class, IEntity
{
    private readonly Dictionary<int, T> items = new();
    public void Add(T item) => items.Add(item.Id, item);
    public T? Find(int id) => items.GetValueOrDefault(id);
}
