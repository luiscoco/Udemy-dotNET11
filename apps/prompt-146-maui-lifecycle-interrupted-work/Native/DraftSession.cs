using Microsoft.Maui.Networking;
namespace NativeCourse;
public sealed class DraftSession : IDisposable
{
    private CancellationTokenSource? operation;
    public string Draft { get; set; } = "My offline course draft";
    public string State { get; private set; } = "Ready";
    public int Commits { get; private set; }
    public event Action? Changed;
    public async Task BeginAsync()
    {
        if (operation is not null) return;
        using var cancellation = new CancellationTokenSource();
        operation = cancellation;
        State = "Working";
        Changed?.Invoke();
        try
        {
            await Task.Delay(600, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Commits++;
            State = "Completed";
        }
        catch (OperationCanceledException) { State = "Interrupted. Draft retained."; }
        finally { operation = null; Changed?.Invoke(); }
    }
    public void Background()
    {
        operation?.Cancel();
        if (operation is null) State = "Backgrounded. Draft retained.";
        Changed?.Invoke();
    }
    public void Resume()
    {
        State = $"Resumed. Connectivity {Connectivity.Current.NetworkAccess}";
        Changed?.Invoke();
    }
    public void ConnectivityChanged(object? sender, ConnectivityChangedEventArgs args)
    {
        if (args.NetworkAccess != NetworkAccess.Internet) Background();
    }
    public void Dispose() => operation?.Cancel();
}
