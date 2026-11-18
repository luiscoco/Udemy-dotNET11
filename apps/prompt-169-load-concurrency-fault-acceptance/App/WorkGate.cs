using Microsoft.AspNetCore.Http;
public sealed class WorkGate : IDisposable
{
    private readonly SemaphoreSlim permits = new(2,2);
    private readonly object sync = new();
    private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int active;
    private int maximum;
    private bool injectFailure;
    public TaskCompletionSource BothEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool InjectFailure { get => Volatile.Read(ref injectFailure); set => Volatile.Write(ref injectFailure,value); }
    public int Maximum { get { lock(sync) return maximum; } }
    public void Release() => release.TrySetResult();
    public async Task<IResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (InjectFailure) return Results.StatusCode(503);
        if (!await permits.WaitAsync(0,cancellationToken)) return Results.StatusCode(503);
        lock(sync)
        {
            active++;
            maximum = Math.Max(maximum,active);
            if (active == 2) BothEntered.TrySetResult();
        }
        try
        {
            await release.Task.WaitAsync(cancellationToken);
            return Results.Ok("Completed");
        }
        finally
        {
            lock(sync) active--;
            permits.Release();
        }
    }
    public void Dispose() => permits.Dispose();
}
