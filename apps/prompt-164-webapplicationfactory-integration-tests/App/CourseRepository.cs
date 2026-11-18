using System.Collections.Concurrent;
public interface ICourseRepository
{
    CourseDto? Find(int id);
    CourseDto Add(string title);
}
public sealed class CourseRepository : ICourseRepository
{
    private readonly ConcurrentDictionary<int,CourseDto> courses = new();
    private int nextId;
    public CourseDto? Find(int id) => courses.GetValueOrDefault(id);
    public CourseDto Add(string title)
    {
        var course = new CourseDto(Interlocked.Increment(ref nextId), title);
        courses[course.Id] = course;
        return course;
    }
}
