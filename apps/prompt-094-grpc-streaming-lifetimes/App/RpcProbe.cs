public sealed class RpcProbe
{
    public TaskCompletionSource DeadlineEntered {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource DeadlineStopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource StreamStopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
}
