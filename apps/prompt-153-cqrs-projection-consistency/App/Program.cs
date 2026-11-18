Directory.CreateDirectory("artifacts");
var file = Path.Combine("artifacts", "cqrs-" + Guid.NewGuid() + ".db");
using var store = new CourseStore(file);
store.Rename("rename-1", "CQRS with .NET 11");
Check.That(store.Query() is null, "the read model is explicitly stale before projection");
if (args.FirstOrDefault() == "stale")
    Check.That(store.Query() == "CQRS with .NET 11", "incorrect expectation: a command immediately refreshes the independent read model");
store.Project();
Check.That(store.Query() == "CQRS with .NET 11", "the projector advances the query view");
store.Rename("rename-1", "This duplicate must not replace the original");
store.Project();
store.Project();
Check.That(store.Query() == "CQRS with .NET 11", "command deduplication and projection checkpoints make replay idempotent");
using var reopened = new CourseStore(file);
Check.That(reopened.Query() == store.Query(), "another connection reads the persisted projection");
Console.WriteLine("database: durable command event, explicit projector and separate query view verified");
