using System.Text;
using RabbitMQ.Client;
await using var fixture=new BrokerFixture();await fixture.StartAsync();
await using var channel=await fixture.ConfirmingChannel();
async Task Publish(string payload,int attempt=0)
{
    var properties=new BasicProperties{Persistent=true,MessageId=payload,Headers=new Dictionary<string,object?>{{"attempt",attempt}}};
    await channel.BasicPublishAsync("",fixture.Queue,mandatory:true,properties,Encoding.UTF8.GetBytes(payload));
}
await Publish("recovered");
await using(var interrupted=await fixture.Connection.CreateChannelAsync())
{
    var delivery=await interrupted.BasicGetAsync(fixture.Queue,autoAck:false);
    Check.That(delivery is not null,"broker delivers a message with acknowledgement pending");
    await interrupted.CloseAsync();
}
var recovered=await channel.BasicGetAsync(fixture.Queue,autoAck:false);
if(args.FirstOrDefault()=="ack")Check.That(recovered is null,"incorrect expectation: an unacknowledged delivery is gone");
Check.That(recovered is not null && recovered.Redelivered,"closing the channel requeues the unacknowledged message");
await channel.BasicAckAsync(recovered!.DeliveryTag,multiple:false);
await Publish("transient");await Publish("poison");
int transientAttempts=0,successful=0,poison=0;
var completed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
await channel.BasicQosAsync(prefetchSize:0,prefetchCount:1,global:false);
var consumer=new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync+=async(_,delivery)=>
{
  try
  {
    var body=Encoding.UTF8.GetString(delivery.Body.Span);
    var attempt=Convert.ToInt32(delivery.BasicProperties.Headers!["attempt"]);
    if(body=="poison")
    {
        await channel.BasicRejectAsync(delivery.DeliveryTag,requeue:false);
        poison++;Console.WriteLine("service: poison message rejected to dead-letter exchange");
    }
    else
    {
        transientAttempts++;
        if(attempt<2)
        {
            await Publish(body,attempt+1); // Confirm retry before acknowledging original.
            await channel.BasicAckAsync(delivery.DeliveryTag,false);
            Console.WriteLine($"service: retry publication confirmed for attempt {attempt+1}");
        }
        else {successful++;await channel.BasicAckAsync(delivery.DeliveryTag,false);}
    }
    if(successful==1 && poison==1)completed.TrySetResult();
  }
  catch(Exception error){completed.TrySetException(error);}
};
var consumerTag=await channel.BasicConsumeAsync(fixture.Queue,autoAck:false,consumer);
try {await completed.Task.WaitAsync(TimeSpan.FromSeconds(15));}
finally {await channel.BasicCancelAsync(consumerTag);}
Check.That(transientAttempts==3 && successful==1,"bounded retry succeeds on the third processing attempt");
BasicGetResult? dead=null;var deadline=DateTime.UtcNow.AddSeconds(5);
do {dead=await channel.BasicGetAsync(fixture.DeadQueue,autoAck:false);if(dead is not null)break;await Task.Delay(10);}while(DateTime.UtcNow<deadline);
Check.That(dead is not null && Encoding.UTF8.GetString(dead.Body.Span)=="poison","dead-letter queue retains the rejected payload");
Check.That(dead!.BasicProperties.Headers!.ContainsKey("x-death"),"broker records dead-letter metadata");
await channel.BasicAckAsync(dead.DeliveryTag,false);
Check.That(await channel.MessageCountAsync(fixture.Queue)==0,"acknowledged work leaves the main queue empty");
Console.WriteLine("service: real broker recovery, confirmed retries, acknowledgements and dead-letter routing verified");
