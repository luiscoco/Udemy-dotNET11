public sealed class LifecycleStore
{
    private int loads, renders, disposed;
    public int Loads => loads;
    public int Renders => renders;
    public int Disposed => disposed;
    public event Action? Changed;
    public async Task<string> LoadAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref loads);
        await Task.Delay(50, cancellationToken);
        return "Component lifecycle lesson";
    }
    public void Rendered() => Interlocked.Increment(ref renders);
    public void Removed() => Interlocked.Increment(ref disposed);
    public void Publish() => Changed?.Invoke();
}
