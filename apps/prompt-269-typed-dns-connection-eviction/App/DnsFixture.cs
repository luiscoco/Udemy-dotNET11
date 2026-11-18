using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Text;
public sealed class DnsFixture:IAsyncDisposable
{
    private readonly UdpClient socket=new(new IPEndPoint(IPAddress.Loopback,53));
    private readonly CancellationTokenSource stop=new();private readonly Task worker;
    public IPEndPoint Endpoint=>(IPEndPoint)socket.Client.LocalEndPoint!;
    public DnsFixture(){worker=Run();}
    private async Task Run(){try{while(!stop.IsCancellationRequested){UdpReceiveResult query;try{query=await socket.ReceiveAsync(stop.Token);}catch(SocketException e) when(e.SocketErrorCode==SocketError.ConnectionReset){continue;}byte[] q=query.Buffer;int end=12;while(q[end]!=0)end+=q[end]+1;end+=5;
        var reply=new List<byte>(q[..end]);reply[2]=0x81;reply[3]=0x80;reply[6]=0;reply[7]=1;reply[10]=0;reply[11]=0;
        reply.AddRange(new byte[]{0xc0,0x0c,0,16,0,1,0,0,0,30,0,10,9});reply.AddRange(Encoding.ASCII.GetBytes("course-v1"));
        await socket.SendAsync(reply.ToArray(),query.RemoteEndPoint,stop.Token);
    }}catch(OperationCanceledException){}}
    public async ValueTask DisposeAsync(){stop.Cancel();await worker;socket.Dispose();stop.Dispose();}
}

