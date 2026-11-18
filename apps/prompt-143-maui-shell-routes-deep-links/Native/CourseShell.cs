namespace NativeCourse;
public sealed class CourseShell : Shell
{
    public CourseShell(MainPage page)
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;
        Items.Add(new ShellContent { Route = "home", Content = page, Title = "Courses" });
        Routing.RegisterRoute("details", typeof(DetailsPage));
    }
}
