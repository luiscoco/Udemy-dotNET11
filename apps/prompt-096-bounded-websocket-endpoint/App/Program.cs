using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
var probe=new SocketProbe();
await using var api=await ApiHarness.StartAsync(_=>{},app=>
{
    app.UseWebSockets(new WebSocketOptions {KeepAliveInterval=TimeSpan.FromSeconds(30),KeepAliveTimeout=TimeSpan.FromSeconds(15)});
    app.Map("/socket",async context=>
    {if(!context.WebSockets.IsWebSocketRequest){context.Response.StatusCode=400;return;}using var socket=await context.WebSockets.AcceptWebSocketAsync();await SocketSession.Run(socket,probe,context.RequestAborted);});
});
using var client=new ClientWebSocket();await client.ConnectAsync(new Uri(api.Client.BaseAddress!.ToString().Replace("http://","ws://")+"socket"),CancellationToken.None);
await probe.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));Check.That(probe.Peak==2,"the third outbound write waits at the two-message channel bound");probe.AllowSends.TrySetResult();
async Task<string> ReceiveUntil(string prefix)
{
    using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(10));byte[] buffer=new byte[128];
    for(int i=0;i<20;i++){var result=await client.ReceiveAsync(buffer,timeout.Token);string text=Encoding.UTF8.GetString(buffer,0,result.Count);if(text.StartsWith(prefix))return text;}
    throw new Exception("Expected message not received");
}
Check.That(await ReceiveUntil("heartbeat")=="heartbeat","the endpoint sends an explicit application heartbeat message");
await ReceiveUntil("welcome");await client.SendAsync("hello"u8.ToArray(),WebSocketMessageType.Text,true,CancellationToken.None);
Check.That(await ReceiveUntil("echo:")=="echo:hello","the single send loop returns the complete application message");
await client.SendAsync(new byte[20],WebSocketMessageType.Text,false,CancellationToken.None);await client.SendAsync(new byte[20],WebSocketMessageType.Text,true,CancellationToken.None);
WebSocketReceiveResult close;byte[] scratch=new byte[128];do{close=await client.ReceiveAsync(scratch,CancellationToken.None);}while(close.MessageType!=WebSocketMessageType.Close);
Console.WriteLine("network: fragmented 40-byte message -> close "+(int)close.CloseStatus!);
if(args.FirstOrDefault()=="oversize")Check.That(close.CloseStatus==WebSocketCloseStatus.NormalClosure,"the message limit applies to the aggregate across fragments");
Check.That(close.CloseStatus==WebSocketCloseStatus.MessageTooBig,"the aggregate limit rejects a fragmented oversized message with 1009");
await probe.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(10));Check.That(probe.Stopped.Task.IsCompletedSuccessfully,"heartbeat and sender tasks finish during cleanup");
