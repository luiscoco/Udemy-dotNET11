namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly DeviceWork work = new();
    public MainPage() => InitializeComponent();
    private async void Inspect(object? sender, EventArgs args) => DeviceLabel.Text = await work.InspectAsync();
    private async void CancelWork(object? sender, EventArgs args) => await CancelAsync();
    private async Task<bool> CancelAsync()
    {
        var file = work.CreateOwnedPath();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { await work.WriteOwnedFileAsync(file, cancellation.Token); }
        catch (OperationCanceledException) { WorkLabel.Text = "Cancelled before file commit."; }
        return File.Exists(file);
    }
    public async Task VerifyAsync(string mode)
    {
        DeviceLabel.Text = await work.InspectAsync();
        LessonChecks.That(DeviceLabel.Text.Contains("WinUI"), "actual MAUI APIs inspect the selected Windows platform");
        LessonChecks.That(DeviceLabel.Text.Contains("Camera permission status"), "the app checks permission status without requesting camera access");
        var committed = await CancelAsync();
        LessonChecks.That(!committed, "a pre-cancelled file operation leaves no committed device file");
        if (mode == "problem")
            LessonChecks.That(committed, "incorrect expectation: cancelled work commits its device file");
        var file = work.CreateOwnedPath();
        await work.WriteOwnedFileAsync(file, CancellationToken.None);
        LessonChecks.That(await File.ReadAllTextAsync(file) == "Synthetic course note", "the permitted uncancelled operation writes only its owned application file");
        WorkLabel.Text = "Cancelled work left no file. A separate permitted write completed.";
    }
}
