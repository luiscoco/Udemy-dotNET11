using Contracts;
namespace Enrollment;
public sealed class EnrollmentService(ICourseCatalog catalog)
{
    public string Enroll(int id, string student)
    {
        var course = catalog.Find(id);
        return course is null ? "Course unavailable" : $"{student} enrolled in {course.Title}";
    }
}
