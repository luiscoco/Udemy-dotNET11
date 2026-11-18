public static class CourseLabel
{
    public static string Format(string? title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return title.Trim();
    }
}
