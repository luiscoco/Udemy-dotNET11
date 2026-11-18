using Course.Rpc;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
var probe=new RpcProbe();var builder=WebApplication.CreateBuilder();builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");builder.WebHost.ConfigureKestrel(o=>o.ConfigureEndpointDefaults(e=>e.Protocols=HttpProtocols.Http2));
builder.Services.AddSingleton(probe);builder.Services.AddGrpc();await using var app=builder.Build();app.MapGrpcService<WorkshopService>();await app.StartAsync();
using var channel=GrpcChannel.ForAddress(app.Urls.Single());var client=new Workshop.WorkshopClient(channel);
var hello=await client.HelloAsync(new Request {Name="learner"});Check.That(hello.Text=="Hello learner","the generated unary client receives one response");
using(var stream=client.Watch(new WatchRequest {Count=3}))
{
    var values=new List<int>();await foreach(var tick in stream.ResponseStream.ReadAllAsync())values.Add(tick.Number);
    Check.That(values.SequenceEqual(new[]{1,2,3}),"the generated streaming client reads every ordered response");
}
using(var slow=client.SlowAsync(new Request(),deadline:DateTime.UtcNow.AddSeconds(3)))
{
    await probe.DeadlineEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
    if(args.FirstOrDefault()=="deadline") {await slow;return;}
    try {await slow;throw new Exception("Expected deadline");}catch(RpcException error)when(error.StatusCode==StatusCode.DeadlineExceeded){Console.WriteLine("network: gRPC deadline -> DEADLINE_EXCEEDED");}
    await probe.DeadlineStopped.Task.WaitAsync(TimeSpan.FromSeconds(10));
}
using(var cancellation=new CancellationTokenSource())
using(var stream=client.Watch(new WatchRequest {Count=3,Hold=true},cancellationToken:cancellation.Token))
{
    Check.That(await stream.ResponseStream.MoveNext(CancellationToken.None),"the cancellable stream delivers its first response");cancellation.Cancel();
    try {await stream.ResponseStream.MoveNext(CancellationToken.None);throw new Exception("Expected cancellation");}catch(RpcException error)when(error.StatusCode==StatusCode.Cancelled){Console.WriteLine("network: gRPC stream -> CANCELLED");}
    await probe.StreamStopped.Task.WaitAsync(TimeSpan.FromSeconds(10));
}
Check.That(probe.DeadlineStopped.Task.IsCompleted && probe.StreamStopped.Task.IsCompleted,"the server observes both deadline and stream cancellation");await app.StopAsync();
