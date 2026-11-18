using Course.Rpc;
using Grpc.Core;
public sealed class WorkshopService(RpcProbe probe) : Workshop.WorkshopBase
{
    public override Task<Reply> Hello(Request request,ServerCallContext context) => Task.FromResult(new Reply {Text="Hello "+request.Name});
    public override async Task Watch(WatchRequest request,IServerStreamWriter<Tick> writer,ServerCallContext context)
    {
        try
        {
            for(int i=1;i<=request.Count;i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();await writer.WriteAsync(new Tick {Number=i},context.CancellationToken);
                if(request.Hold && i==1) await Task.Delay(Timeout.InfiniteTimeSpan,context.CancellationToken);
            }
        }
        finally {if(request.Hold)probe.StreamStopped.TrySetResult();}
    }
    public override async Task<Reply> Slow(Request request,ServerCallContext context)
    {
        probe.DeadlineEntered.TrySetResult();
        try {await Task.Delay(Timeout.InfiniteTimeSpan,context.CancellationToken);return new Reply();}
        finally {probe.DeadlineStopped.TrySetResult();}
    }
}
