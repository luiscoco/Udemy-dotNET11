public sealed class Pipeline
{
    public int InFlight { get; private set; }
    public int Peak { get; private set; }
    public IAsyncEnumerable<int> Create(Feed feed) => feed.Read()
        .Where(value => value % 2 == 0)
        .Select(async (value, cancellationToken) =>
        {
            InFlight++;
            Peak = Math.Max(Peak, InFlight);
            try { await Task.Delay(1, cancellationToken); return value * 10; }
            finally { InFlight--; }
        });
}
