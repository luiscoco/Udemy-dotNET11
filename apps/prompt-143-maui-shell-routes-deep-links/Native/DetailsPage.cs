namespace NativeCourse;
public sealed class DetailsPage : ContentPage, IQueryAttributable
{
    private readonly CourseSelection selection;
    private readonly Label heading = new() { FontSize = 30, FontAttributes = FontAttributes.Bold };
    public int CourseId { get; private set; }
    public DetailsPage(CourseSelection selection)
    {
        this.selection = selection;
        BackgroundColor = Color.FromArgb("#F4F2FB");
        var back = new Button { Text = "Back to courses", HorizontalOptions = LayoutOptions.Start };
        back.Clicked += async (_, _) => await Shell.Current.GoToAsync("..");
        Content = new VerticalStackLayout
        {
            Padding = 36, Spacing = 24,
            Children = { heading, new Label { Text = "The detail page receives a validated route value.", FontSize = 20 }, back }
        };
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("id", out var raw) || raw is not int id || id <= 0)
            throw new ArgumentException("A validated course identifier is required.");
        CourseId = id;
        selection.CourseId = id;
        heading.Text = $"Course {id}";
        Title = $"Course {id}";
    }
}
