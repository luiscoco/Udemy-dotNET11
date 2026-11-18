namespace NativeCourse;
public sealed class CourseNavigation
{
    public static int Parse(Uri link)
    {
        if (link.Scheme != "course" || link.Host != "lesson" ||
            !int.TryParse(link.AbsolutePath.Trim('/'), out var id) || id <= 0)
            throw new ArgumentException("Use a course://lesson/{positive-id} link.");
        return id;
    }
    public Task OpenAsync(Uri link)
    {
        var id = Parse(link);
        return Shell.Current.GoToAsync("details", new Dictionary<string, object> { ["id"] = id });
    }
}
