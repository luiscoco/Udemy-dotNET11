using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
public sealed class InvocationProbe
{
    public TaskCompletionSource Entered {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Stopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
}
[Authorize]
public sealed class RefreshHub(InvocationProbe probe) : Hub
{
    public string ReadState()=>Context.User!.FindFirst("revision")!.Value;
    [Authorize(Roles="editor")]
    public string Edit()=>"editor allowed";
    public async Task LongWork(CancellationToken cancellationToken)
    {
        probe.Entered.TrySetResult();
        try {await Task.Delay(Timeout.InfiniteTimeSpan,cancellationToken);}
        catch(OperationCanceledException)when(cancellationToken.IsCancellationRequested){probe.Stopped.TrySetResult();}
    }
}
