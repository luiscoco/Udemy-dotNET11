public sealed class ListMetrics
{
    private int requests, items;
    public int Requests => requests;
    public int Items => items;
    public void Record(int count)
    {
        Interlocked.Increment(ref requests);
        Interlocked.Add(ref items, count);
    }
}
