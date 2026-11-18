var builder = DistributedApplication.CreateBuilder(args);
var database = builder.AddPostgres("postgres").WithImageTag("17-alpine")
    .AddDatabase("coursedb");
var cache = builder.AddRedis("cache").WithImageTag("7-alpine");
var api = builder.AddProject<Projects.Api>("api")
    .WithHttpEndpoint(name: "http")
    .WithReference(database).WithReference(cache)
    .WaitFor(database).WaitFor(cache)
    .WithHttpHealthCheck("/health");
builder.AddProject<Projects.Worker>("worker")
    .WithReference(database).WithReference(cache)
    .WaitFor(database).WaitFor(cache);
builder.AddProject<Projects.Caller>("caller")
    .WithHttpEndpoint(name: "http")
    .WithReference(api).WaitFor(api)
    .WithHttpHealthCheck("/health");
builder.Build().Run();
