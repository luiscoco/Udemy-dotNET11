namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly CourseNavigation navigation;
    private readonly CourseSelection selection;
    public MainPage(CourseNavigation navigation, CourseSelection selection)
    {
        InitializeComponent();
        this.navigation = navigation;
        this.selection = selection;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        SelectionLabel.Text = selection.CourseId is int id ? $"Selected course {id}" : "No course selected.";
    }
    private async void Open(object? sender, EventArgs args)
    {
        try { await navigation.OpenAsync(new Uri(LinkEntry.Text)); }
        catch (ArgumentException exception) { SelectionLabel.Text = exception.Message; }
        catch (UriFormatException) { SelectionLabel.Text = "Enter a valid course link."; }
    }
    public async Task VerifyAsync(string mode)
    {
        bool rejected = false;
        try { CourseNavigation.Parse(new Uri("https://example.test/42")); }
        catch (ArgumentException) { rejected = true; }
        LessonChecks.That(rejected, "the deep-link parser rejects an external scheme and host");
        if (mode == "problem")
            LessonChecks.That(!rejected, "incorrect expectation: an external URL is an accepted course deep link");
        await navigation.OpenAsync(new Uri("course://lesson/42"));
        LessonChecks.That(Shell.Current.CurrentPage is DetailsPage { CourseId: 42 }, "Shell navigates to the actual detail page with its typed route value");
        await NativeCapture.SaveAsync(Path.Combine(Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!, "native-details.png"));
        await Shell.Current.GoToAsync("..");
        await navigation.OpenAsync(new Uri("course://lesson/43"));
        LessonChecks.That(Shell.Current.CurrentPage is DetailsPage { CourseId: 43 }, "a later navigation receives the new identifier instead of stale detail state");
        await Shell.Current.GoToAsync("..");
        LessonChecks.That(selection.CourseId == 43 && SelectionLabel.Text.Contains("43"), "back navigation retains the explicit selected course in the landing page");
    }
}
