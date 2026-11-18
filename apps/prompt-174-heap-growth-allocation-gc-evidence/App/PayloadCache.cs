using System.Runtime.CompilerServices;
public sealed class CoursePayload
{
    public byte[] Data { get; } = new byte[256 * 1024];
}
public static class PayloadCache
{
    private static readonly List<CoursePayload> retained = [];
    public static int Count => retained.Count;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Load()
    {
        for(int i=0;i<40;i++) retained.Add(new CoursePayload());
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Release() => retained.Clear();
}
