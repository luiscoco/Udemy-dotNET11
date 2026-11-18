using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
public sealed class SocketProbe
{
    public TaskCompletionSource Blocked {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource AllowSends {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Stopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Peak;
}
public static class SocketSession
{
    public static async Task Run(WebSocket socket,SocketProbe probe,CancellationToken requestToken)
    {
        using var stop=CancellationTokenSource.CreateLinkedTokenSource(requestToken);var ct=stop.Token;
        var outbound=Channel.CreateBounded<string>(new BoundedChannelOptions(2){FullMode=BoundedChannelFullMode.Wait});
        async Task Enqueue(string text) {var write=outbound.Writer.WriteAsync(text,ct);if(!write.IsCompleted)probe.Blocked.TrySetResult();await write;probe.Peak=Math.Max(probe.Peak,outbound.Reader.Count);}
        async Task Send()
        {
            await probe.AllowSends.Task.WaitAsync(ct);
            await foreach(var text in outbound.Reader.ReadAllAsync(ct))await socket.SendAsync(Encoding.UTF8.GetBytes(text),WebSocketMessageType.Text,true,ct);
        }
        async Task Heartbeat()
        {
            using var timer=new PeriodicTimer(TimeSpan.FromSeconds(1));
            while(await timer.WaitForNextTickAsync(ct))await Enqueue("heartbeat");
        }
        Task sender=Send(),heartbeat=Heartbeat();
        try
        {
            await Enqueue("heartbeat");await Enqueue("ready");await Enqueue("welcome");
            byte[] part=new byte[16];using var message=new MemoryStream();
            while(socket.State==WebSocketState.Open)
            {
                var result=await socket.ReceiveAsync(part,ct);
                if(result.MessageType==WebSocketMessageType.Close)break;
                if(message.Length+result.Count>32){await socket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig,"32-byte aggregate limit",CancellationToken.None);break;}
                message.Write(part,0,result.Count);
                if(result.EndOfMessage){await Enqueue("echo:"+Encoding.UTF8.GetString(message.ToArray()));message.SetLength(0);}
            }
        }
        finally
        {
            stop.Cancel();outbound.Writer.TryComplete();
            try{await Task.WhenAll(sender,heartbeat);}catch(OperationCanceledException)when(ct.IsCancellationRequested){}
            probe.Stopped.TrySetResult();
        }
    }
}
