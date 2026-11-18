namespace NativeCourse;
public sealed class RatingBridge
{
    public int LastRating { get; private set; }
    internal event Action<string>? Rated;
    public string Invoke(string operation, string courseId, int rating)
    {
        if (operation != "rate") return "Rejected operation.";
        if (courseId != "course-11" || rating is < 1 or > 5)
            return "Rejected rating. Choose this course and a value from one to five.";
        LastRating = rating;
        var result = $"Recorded {rating} stars for the .NET 11 course.";
        Rated?.Invoke(result);
        return result;
    }
}
