using Microsoft.Maui.Networking;
namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly DraftSession session;
    public MainPage(DraftSession session)
    {
        InitializeComponent();
        this.session = session;
        session.Changed += Update;
        Loaded += Attach;
        Unloaded += Detach;
    }
    private void Attach(object? sender, EventArgs args)
    {
        Window.Stopped += Backgrounded;
        Window.Resumed += Resumed;
        Connectivity.Current.ConnectivityChanged += session.ConnectivityChanged;
    }
    private void Detach(object? sender, EventArgs args)
    {
        Window.Stopped -= Backgrounded;
        Window.Resumed -= Resumed;
        Connectivity.Current.ConnectivityChanged -= session.ConnectivityChanged;
        session.Changed -= Update;
        session.Background();
    }
    private void Update() => MainThread.BeginInvokeOnMainThread(() => StateLabel.Text = session.State);
    private async void Begin(object? sender, EventArgs args) => await session.BeginAsync();
    private void Pause(object? sender, EventArgs args) => session.Background();
    private void Resume(object? sender, EventArgs args) => session.Resume();
    private void Backgrounded(object? sender, EventArgs args) => session.Background();
    private void Resumed(object? sender, EventArgs args) => session.Resume();
    public async Task VerifyAsync(string mode)
    {
        session.Draft = DraftEntry.Text;
        var pending = session.BeginAsync();
        await Task.Delay(25);
        session.Background();
        await pending;
        LessonChecks.That(session.Commits == 0, "background cancellation prevents the interrupted operation from committing");
        LessonChecks.That(session.Draft == DraftEntry.Text, "the draft remains available after interruption");
        if (mode == "problem")
            LessonChecks.That(session.Commits == 1, "incorrect expectation: backgrounded work commits its unfinished result");
        session.Resume();
        await session.BeginAsync();
        LessonChecks.That(session.Commits == 1, "an explicit resumed operation completes once");
        LessonChecks.That(StateLabel.Text == "Completed", "the actual native status control displays the recovered operation");
    }
}
