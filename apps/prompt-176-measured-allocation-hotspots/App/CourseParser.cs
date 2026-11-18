using System.Buffers;
public readonly record struct CourseTotals(int Count, int Sum);
public static class CourseParser
{
    public static CourseTotals Allocating(string input)
    {
        string[] fields = input.Split(',');
        int sum = 0;
        foreach (string field in fields) sum += int.Parse(field);
        return new(fields.Length, sum);
    }
    public static CourseTotals Buffered(string input)
    {
        int capacity = input.AsSpan().Count(',') + 1;
        int[] buffer = ArrayPool<int>.Shared.Rent(capacity);
        try
        {
            int count = 0, sum = 0;
            foreach (Range range in input.AsSpan().Split(','))
            {
                int value = int.Parse(input.AsSpan()[range]);
                buffer[count++] = value;
            }
            for(int index=0;index<count;index++) sum += buffer[index];
            return new(count, sum);
        }
        finally { ArrayPool<int>.Shared.Return(buffer); }
    }
}
