using System.Numerics;
public static class NumericKernel
{
    public static int Scalar(ReadOnlySpan<int> data)
    {
        int sum = 0;
        foreach (int value in data) sum = unchecked(sum + value);
        return sum;
    }
    public static int Sum(ReadOnlySpan<int> data, bool forceScalar = false, bool omitTail = false)
    {
        if (forceScalar || !Vector.IsHardwareAccelerated) return Scalar(data);
        int index = 0;
        var total = Vector<int>.Zero;
        for (; index <= data.Length - Vector<int>.Count; index += Vector<int>.Count)
            total += new Vector<int>(data.Slice(index, Vector<int>.Count));
        int sum = Vector.Sum(total);
        if (!omitTail)
            for (; index < data.Length; index++) sum = unchecked(sum + data[index]);
        return sum;
    }
}
