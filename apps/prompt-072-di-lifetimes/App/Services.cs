public abstract class Stamp : IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool Disposed { get; private set; }
    public void Dispose() => Disposed = true;
}
public sealed class SingletonStamp : Stamp;
public sealed class ScopedStamp : Stamp;
public sealed class TransientStamp : Stamp;
public sealed class RequestService(SingletonStamp singleton, ScopedStamp scoped)
{
    public SingletonStamp Singleton => singleton;
    public ScopedStamp Scoped => scoped;
}
public sealed class BadCache(ScopedStamp scoped)
{
    public ScopedStamp Captured => scoped;
}
