using System.Net;
using Microsoft.Azure.Cosmos;
var options=new CosmosClientOptions
{
    ConnectionMode=ConnectionMode.Gateway,
    ConsistencyLevel=ConsistencyLevel.Session,
    LimitToEndpoint=true
};
// Local emulator only: authentication is not enforced by this emulator.
using var client=new CosmosClient("http://127.0.0.1:18081/",
    Convert.ToBase64String(new byte[64]),options);
var databaseId="course-"+Guid.NewGuid().ToString("N");
var database=(await client.CreateDatabaseAsync(databaseId)).Database;
try
{
    var properties=new ContainerProperties("orders","/tenantId");
    var container=(await database.CreateContainerAsync(properties,throughput:400)).Container;
    var key=new PartitionKey("tenant-a");
    using(var batch=await container.CreateTransactionalBatch(key)
        .CreateItem(new Document("order-1","tenant-a","order",10))
        .CreateItem(new Document("audit-1","tenant-a","audit",10)).ExecuteAsync())
        Check.That(batch.IsSuccessStatusCode,"same-partition order and audit commit together");
    var order=await container.ReadItemAsync<Document>("order-1",key);
    Check.That(order.Resource.amount==10,"point read supplies both id and partition key");
    bool absent=false;
    try{await container.ReadItemAsync<Document>("order-1",new PartitionKey("tenant-b"));}
    catch(CosmosException e) when(e.StatusCode==HttpStatusCode.NotFound){absent=true;}
    if(args.FirstOrDefault()=="partition")Check.That(!absent,"incorrect expectation: item id alone crosses partitions");
    Check.That(absent,"wrong partition key does not locate tenant-a's order");
    using(var rejected=await container.CreateTransactionalBatch(key)
        .CreateItem(new Document("order-2","tenant-a","order",20))
        .CreateItem(new Document("audit-1","tenant-a","duplicate",20)).ExecuteAsync())
        Check.That(!rejected.IsSuccessStatusCode,"duplicate audit rejects the transactional batch");
    bool rolledBack=false;
    try{await container.ReadItemAsync<Document>("order-2",key);}
    catch(CosmosException e) when(e.StatusCode==HttpStatusCode.NotFound){rolledBack=true;}
    Check.That(rolledBack,"failed batch leaves no partially committed order");
    using(var cross=await container.CreateTransactionalBatch(key)
        .CreateItem(new Document("wrong-tenant","tenant-b","order",1)).ExecuteAsync())
        Check.That(!cross.IsSuccessStatusCode,"one batch cannot write an item in another logical partition");
    Console.WriteLine("database: partition reads and batch atomicity observed on local emulator");
    Console.WriteLine("database: configured Session / 400 RU/s; emulator does not verify production consistency or RU cost");
}
finally{await database.DeleteAsync();}
public sealed record Document(string id,string tenantId,string kind,int amount);
