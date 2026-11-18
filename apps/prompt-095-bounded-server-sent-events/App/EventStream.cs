using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
public sealed record Notice(int Number);
public sealed class EventProbe
{
    public int Peak; public TaskCompletionSource Stopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
}
public static class EventStream
{
    public static async IAsyncEnumerable<SseItem<Notice>> Read(EventProbe probe,bool hold,[EnumeratorCancellation]CancellationToken token)
    {
        using var stop=CancellationTokenSource.CreateLinkedTokenSource(token);
        var queue=Channel.CreateBounded<int>(new BoundedChannelOptions(2){FullMode=BoundedChannelFullMode.Wait});
        var blocked=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task Produce()
        {
            try
            {
                for(int i=1;i<=6;i++)
                {
                    var write=queue.Writer.WriteAsync(i,stop.Token);if(!write.IsCompleted)blocked.TrySetResult();await write;
                    probe.Peak=Math.Max(probe.Peak,queue.Reader.Count);
                    if(hold && i==1)await Task.Delay(Timeout.InfiniteTimeSpan,stop.Token);
                }
            }
            finally {queue.Writer.TryComplete();}
        }
        Task producer=Produce();
        try
        {
            if(!hold)await blocked.Task.WaitAsync(token);
            await foreach(int value in queue.Reader.ReadAllAsync(token))yield return new SseItem<Notice>(new(value)){EventId=value.ToString()};
        }
        finally {stop.Cancel();try{await producer;}catch(OperationCanceledException)when(stop.IsCancellationRequested){}probe.Stopped.TrySetResult();}
    }
}
