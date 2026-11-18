public sealed class AccessWindow(TimeProvider clock, TimeSpan lifetime)
{
    private readonly DateTimeOffset expiresAt = clock.GetUtcNow() + lifetime;
    public bool IsValid => clock.GetUtcNow() < expiresAt;
    public Task WaitForExpiryAsync(CancellationToken cancellationToken = default)
    {
        var remaining = expiresAt - clock.GetUtcNow();
        return remaining <= TimeSpan.Zero ? Task.CompletedTask : Task.Delay(remaining, clock, cancellationToken);
    }
}
