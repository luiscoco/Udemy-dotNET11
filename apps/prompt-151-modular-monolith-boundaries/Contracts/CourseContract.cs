namespace Contracts;
public sealed record CourseSummary(int Id, string Title);
public interface ICourseCatalog
{
    CourseSummary? Find(int id);
}
