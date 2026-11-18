public sealed class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Seats { get; set; }
    public int Revision { get; set; }
    public List<Enrollment> Enrollments { get; set; } = [];
}
public sealed class Enrollment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Student { get; set; } = "";
    public Course Course { get; set; } = null!;
}
