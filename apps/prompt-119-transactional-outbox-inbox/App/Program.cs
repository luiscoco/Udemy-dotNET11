using StackExchange.Redis;
using var redis=await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("COURSE_REDIS")??"127.0.0.1:16379");
var broker=redis.GetDatabase();var stream="course119:"+Guid.NewGuid().ToString("N");
Directory.CreateDirectory("artifacts");var prefix=Path.GetFullPath(Path.Combine("artifacts",Guid.NewGuid().ToString("N")));
try
{
    using(var producer=new OutboxStore(prefix+"-outbox.db"))
    {
        try{producer.CreateOrder("rolled-back",7,fail:true);}catch(InvalidOperationException){}
        Check.That(producer.OrderCount==0 && producer.Pending().Count==0,"business write and outbox row roll back together");
        producer.CreateOrder("order-1",12);
        Check.That(producer.OrderCount==1 && producer.Pending().Count==1,"business order and outbox message commit in one local transaction");
        var row=producer.Pending().Single();
        await broker.StreamAddAsync(stream,[new("messageId",row.Id),new("amount",row.Amount)]);
        // Simulated crash after publication: do not mark this outbox row sent.
    }
    using(var restarted=new OutboxStore(prefix+"-outbox.db"))
    {
        var row=restarted.Pending().Single();
        await broker.StreamAddAsync(stream,[new("messageId",row.Id),new("amount",row.Amount)]);
        restarted.MarkSent(row.Id);
        Check.That(restarted.Pending().Count==0,"restarted dispatcher publishes the still-pending row and then marks it sent");
    }
    var deliveries=await broker.StreamRangeAsync(stream);
    Check.That(deliveries.Length==2,"publication gap produces two actual broker deliveries");
    using(var consumer=new InboxStore(prefix+"-inbox.db"))
    {
        foreach(var delivery in deliveries)
        {
            var id=delivery.Values.Single(x=>x.Name=="messageId").Value.ToString();
            var amount=(int)delivery.Values.Single(x=>x.Name=="amount").Value;
            Console.WriteLine($"service: inbox accepted={consumer.Apply(id,amount)} for {id}");
        }
        if(args.FirstOrDefault()=="duplicate")Check.That(consumer.Total==24,"incorrect expectation: duplicate deliveries imply duplicate effects");
        Check.That(consumer.Total==12,"transactional inbox commits exactly one business effect for the repeated message id");
    }
    using(var reopened=new InboxStore(prefix+"-inbox.db"))
        Check.That(!reopened.Apply("order-1",12) && reopened.Total==12,"reopened inbox retains deduplication evidence");
    Console.WriteLine("service: local atomicity plus at-least-once transport and idempotency verified; no distributed exactly-once claim");
}
finally{await broker.KeyDeleteAsync(stream);foreach(var suffix in new[]{"-outbox.db","-inbox.db"})if(File.Exists(prefix+suffix))File.Delete(prefix+suffix);}
