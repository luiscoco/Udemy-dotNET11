using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

public sealed record Job(string Tenant, ActivityContext Parent);
public sealed class OperationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.User!.FindFirst("tenant")!.Value);
        await base.OnConnectedAsync();
    }
}

public sealed class Operations : IDisposable
{
    public readonly Channel<Job> Queue = Channel.CreateBounded<Job>(8);
    public readonly ConcurrentDictionary<string, int> Counts = new();
    public readonly ActivitySource Traces = new("Course.Operations");
    public readonly Meter Meter = new("Course.Operations");
    public int Completed;
    public bool Broken;
    public void Dispose()
    {
        Traces.Dispose();
        Meter.Dispose();
    }
}

public sealed class OperationsWorker(Operations state, IMemoryCache cache, IHubContext<OperationsHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var counter = state.Meter.CreateCounter<long>("course.jobs.completed");
        await foreach (var job in state.Queue.Reader.ReadAllAsync(token))
        {
            using var activity = state.Traces.StartActivity("process-job", ActivityKind.Consumer, job.Parent);
            int value = state.Counts.AddOrUpdate(job.Tenant, 1, (_, v) => v + 1);
            if (!state.Broken)
                cache.Remove(job.Tenant);
            Interlocked.Increment(ref state.Completed);
            counter.Add(1);
            await hub.Clients.Group(job.Tenant).SendAsync("changed", value, token);
        }
    }
}
