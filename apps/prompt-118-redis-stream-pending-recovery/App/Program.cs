using StackExchange.Redis;
using var redis=await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("COURSE_REDIS")??"127.0.0.1:16379");
var db=redis.GetDatabase();var stream="course118:"+Guid.NewGuid().ToString("N");const string group="workers";
Directory.CreateDirectory("artifacts");var file=Path.GetFullPath(Path.Combine("artifacts",Guid.NewGuid()+".db"));
try
{
    await db.StreamCreateConsumerGroupAsync(stream,group,"0-0",createStream:true);
    var id=await db.StreamAddAsync(stream,[new("messageId","reserve-1"),new("amount",1)]);
    var original=await db.StreamReadGroupAsync(stream,group,"worker-a",">",count:1);
    Check.That(original.Length==1,"worker-a receives the new stream entry");
    using(var beforeCrash=new InboxStore(file))Check.That(beforeCrash.Apply("reserve-1",1),"business effect commits before the acknowledgement gap");
    var pending=await db.StreamPendingAsync(stream,group);
    if(args.FirstOrDefault()=="pending")Check.That(pending.PendingMessageCount==0,"incorrect expectation: delivery automatically acknowledges work");
    Check.That(pending.PendingMessageCount==1,"unacknowledged delivery stays in the group's pending list");
    // Zero idle time makes this isolated recovery fixture deterministic; production uses a real timeout.
    var claimed=await db.StreamAutoClaimAsync(stream,group,"worker-b",0,"0-0",count:10);
    Check.That(claimed.ClaimedEntries.Length==1 && claimed.ClaimedEntries[0].Id==id,"worker-b claims the pending entry from worker-a");
    using(var restarted=new InboxStore(file))
    {
        foreach(var entry in claimed.ClaimedEntries)
        {
            var message=entry.Values.Single(x=>x.Name=="messageId").Value.ToString();
            var amount=(int)entry.Values.Single(x=>x.Name=="amount").Value;
            Check.That(!restarted.Apply(message,amount),"persisted inbox suppresses repeated business effect after reopening storage");
            await db.StreamAcknowledgeAsync(stream,group,entry.Id);
        }
        // Republished business message: another Redis entry id, same stable business id.
        await db.StreamAddAsync(stream,[new("messageId","reserve-1"),new("amount",1)]);
        var duplicate=await db.StreamReadGroupAsync(stream,group,"worker-b",">",count:1);
        Check.That(duplicate.Single().Id!=id,"duplicate business message can have a different stream entry id");
        Check.That(!restarted.Apply("reserve-1",1) && restarted.Total==1,"business id deduplication preserves one durable effect");
        await db.StreamAcknowledgeAsync(stream,group,duplicate.Single().Id);
    }
    Check.That((await db.StreamPendingAsync(stream,group)).PendingMessageCount==0,"acknowledgements clear recovered and duplicate pending entries");
    Console.WriteLine("service: real Redis consumer group and reopened SQLite inbox verified");
}
finally{await db.KeyDeleteAsync(stream);if(File.Exists(file))File.Delete(file);}
