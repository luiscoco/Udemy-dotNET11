public interface IEntity { int Id { get; } }
public class Document(int id) : IEntity { public int Id { get; } = id; }
public sealed class Invoice(int id, decimal net) : Document(id)
{
    public decimal Net { get; } = net;
}

public interface ISource<out T> { T Read(); }
public interface ISink<in T> { void Write(T item); }

public sealed class SingleSource<T>(T value) : ISource<T>
{
    public T Read() => value;
}

public sealed class DocumentSink : ISink<Document>
{
    public int LastId { get; private set; }
    public void Write(Document item) => LastId = item.Id;
}
