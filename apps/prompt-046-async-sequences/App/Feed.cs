using System.Runtime.CompilerServices;
public sealed class Feed
{
    public int Produced { get; private set; }
    public int Readers { get; private set; }
    public int Disposals { get; private set; }
    public async IAsyncEnumerable<int> Read([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Readers++;
        try
        {
            for (int value = 1; value <= 100; value++)
            {
                await Task.Delay(1, cancellationToken);
                Produced++;
                yield return value;
            }
        }
        finally { Readers--; Disposals++; }
    }
}
