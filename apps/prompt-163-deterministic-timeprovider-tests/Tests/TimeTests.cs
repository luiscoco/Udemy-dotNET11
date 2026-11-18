using Microsoft.Extensions.Time.Testing;
[TestClass]
public sealed class TimeTests
{
    private static FakeTimeProvider Clock() => new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    [TestMethod]
    public void Access_expires_at_the_exact_boundary()
    {
        var clock = Clock();
        var window = new AccessWindow(clock, TimeSpan.FromMinutes(10));
        clock.Advance(TimeSpan.FromMinutes(10) - TimeSpan.FromTicks(1));
        Assert.IsTrue(window.IsValid);
        clock.Advance(TimeSpan.FromTicks(1));
        Assert.IsFalse(window.IsValid, "Access must expire exactly at its deadline.");
    }
    [TestMethod]
    public async Task Fake_time_completes_the_scheduled_delay()
    {
        var clock = Clock();
        var window = new AccessWindow(clock, TimeSpan.FromHours(1));
        Task waiting = window.WaitForExpiryAsync();
        Assert.IsFalse(waiting.IsCompleted);
        clock.Advance(TimeSpan.FromHours(1));
        await waiting.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(window.IsValid);
    }
    [TestMethod]
    public async Task Caller_cancellation_remains_observable()
    {
        var clock = Clock();
        var window = new AccessWindow(clock, TimeSpan.FromHours(1));
        using var cancellation = new CancellationTokenSource();
        Task waiting = window.WaitForExpiryAsync(cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await waiting);
    }
}
