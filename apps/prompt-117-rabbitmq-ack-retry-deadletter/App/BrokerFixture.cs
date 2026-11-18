using RabbitMQ.Client;
public sealed class BrokerFixture : IAsyncDisposable
{
    public IConnection Connection {get;private set;}=null!;
    public string Queue {get;}="course117."+Guid.NewGuid().ToString("N");
    public string DeadQueue=>Queue+".dead";
    public string DeadExchange=>Queue+".dlx";
    public async Task StartAsync()
    {
        var factory=new ConnectionFactory{HostName="127.0.0.1",Port=15673,UserName="course",Password="course-local"};
        Connection=await factory.CreateConnectionAsync();
        await using var channel=await Connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(DeadExchange,ExchangeType.Direct,durable:true);
        await channel.QueueDeclareAsync(DeadQueue,durable:true,exclusive:false,autoDelete:false);
        await channel.QueueBindAsync(DeadQueue,DeadExchange,"failed");
        await channel.QueueDeclareAsync(Queue,durable:true,exclusive:false,autoDelete:false,
            arguments:new Dictionary<string,object?>{{"x-dead-letter-exchange",DeadExchange},{"x-dead-letter-routing-key","failed"}});
    }
    public Task<IChannel> ConfirmingChannel()=>Connection.CreateChannelAsync(
        new CreateChannelOptions(publisherConfirmationsEnabled:true,publisherConfirmationTrackingEnabled:true));
    public async ValueTask DisposeAsync()
    {
        if(Connection is null)return;
        await using var channel=await Connection.CreateChannelAsync();
        await channel.QueueDeleteAsync(Queue);await channel.QueueDeleteAsync(DeadQueue);await channel.ExchangeDeleteAsync(DeadExchange);
        await Connection.DisposeAsync();
    }
}
