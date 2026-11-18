using System.Collections;
public sealed class ObservedLines(Line[] lines) : IEnumerable<Line>
{
    public int Visits { get; private set; }
    public IEnumerator<Line> GetEnumerator()
    {
        foreach (var line in lines)
        {
            Visits++;
            yield return line;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
