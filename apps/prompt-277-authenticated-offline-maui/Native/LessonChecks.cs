namespace NativeCourse;
public static class LessonChecks
{
    public static List<string> Messages { get; } = [];

    public static void That(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
        Messages.Add(message);
    }
}

public interface ILessonPage
{
    Task VerifyAsync(string mode);
}
