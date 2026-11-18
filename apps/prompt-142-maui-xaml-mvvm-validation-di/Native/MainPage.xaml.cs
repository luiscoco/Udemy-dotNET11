namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly CourseViewModel viewModel;
    private readonly CourseRepository repository;
    public MainPage(CourseViewModel viewModel, CourseRepository repository)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.repository = repository;
        BindingContext = viewModel;
    }
    public async Task VerifyAsync(string mode)
    {
        LessonChecks.That(viewModel.HasErrors && !SaveButton.IsEnabled,
            "validation disables the bound command for an empty title");
        if (mode == "problem")
            LessonChecks.That(SaveButton.IsEnabled, "incorrect expectation: an invalid title enables Save");
        TitleEntry.Text = "Practical .NET 11";
        await Task.Delay(50);
        LessonChecks.That(viewModel.Title == TitleEntry.Text, "the XAML two-way binding updates the view model");
        LessonChecks.That(SaveButton.IsEnabled, "a valid title enables the command");
        ((IButtonController)SaveButton).SendClicked();
        LessonChecks.That(repository.LastTitle == "Practical .NET 11", "the command saves through the injected repository");
        LessonChecks.That(StatusLabel.Text.Contains("Saved"), "property notification updates the visible result");
    }
}
