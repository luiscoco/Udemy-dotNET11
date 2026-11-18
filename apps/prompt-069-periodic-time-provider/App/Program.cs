using Microsoft.Extensions.Time.Testing;
var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
if (args.FirstOrDefault() == "catchup")
{
    using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10), clock);
    clock.Advance(TimeSpan.FromSeconds(100));
    int available = await timer.WaitForNextTickAsync() ? 1 : 0;
    Task<bool> next = timer.WaitForNextTickAsync().AsTask();
    Console.WriteLine($"Ten elapsed periods: first tick count {available}; another immediately ready {next.IsCompleted}");
    timer.Dispose();
    _ = await next;
    Check.That(available == 10, "expected ten queued ticks after missing ten periods");
    return;
}
int active = 0, peak = 0;
using var loop = new PeriodicWork(clock, async token =>
{
    peak = Math.Max(peak, ++active);
    try { token.ThrowIfCancellationRequested(); await Task.Yield(); }
    finally { active--; }
});
using var cancellation = new CancellationTokenSource();
if (args.FirstOrDefault() == "cancel")
{
    cancellation.Cancel();
    _ = await loop.TickOnceAsync(cancellation.Token);
    return;
}
for (int i = 0; i < 3; i++)
{
    Task<bool> tick = loop.TickOnceAsync(cancellation.Token);
    clock.Advance(TimeSpan.FromSeconds(10));
    Check.That(await tick, "advancing injected time completes one awaited tick");
}
Check.That(loop.Completed == 3 && peak == 1, "awaiting each work item keeps periodic execution sequential");
Check.That(loop.LastTick == new DateTimeOffset(2026, 1, 1, 0, 0, 30, TimeSpan.Zero), "the observed timestamp comes from the injected clock");
Task<bool> pending = loop.TickOnceAsync(CancellationToken.None);
loop.Dispose();
Check.That(!await pending, "disposing the timer completes a pending tick with false");
Console.WriteLine($"Completed {loop.Completed}; peak work {peak}; final injected time {loop.LastTick:O}");
