namespace NativeCourse;
public sealed class CourseRepository
{
    public string? LastTitle { get; private set; }
    public void Save(string title) => LastTitle = title;
}
