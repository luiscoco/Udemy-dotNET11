using Microsoft.Data.Sqlite;
namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly NoteRepository repository;
    private readonly NoteSync sync;
    public MainPage(NoteRepository repository, NoteSync sync)
    {
        InitializeComponent();
        this.repository = repository;
        this.sync = sync;
        Show();
    }
    private void Show()
    {
        var note = repository.Read();
        StoredLabel.Text = $"Stored title {note.Title}\nServer version {note.Version}\nPending edit {note.OperationId is not null}";
    }
    private void SaveOffline(object? sender, EventArgs args)
    {
        repository.Edit(TitleEntry.Text);
        StateLabel.Text = "Saved locally. Awaiting synchronization.";
        Show();
    }
    private async void Synchronize(object? sender, EventArgs args)
    {
        try { StateLabel.Text = await sync.SynchronizeAsync(); }
        catch (HttpRequestException) { StateLabel.Text = "Server unavailable. The local edit is retained."; }
        catch (TaskCanceledException) { StateLabel.Text = "Synchronization interrupted. The local edit is retained."; }
        Show();
    }
    private void AcceptServer(object? sender, EventArgs args)
    {
        repository.AcceptServer();
        TitleEntry.Text = repository.Read().Title;
        StateLabel.Text = "Resolved using the server revision.";
        Show();
    }
    public async Task VerifyAsync(string mode)
    {
        SaveOffline(this,EventArgs.Empty);
        using (var reopened = new SqliteConnection($"Data Source={repository.FilePath}"))
        {
            reopened.Open();
            using var command = reopened.CreateCommand();
            command.CommandText = "SELECT Title FROM Notes WHERE Id=1";
            LessonChecks.That((string)command.ExecuteScalar()! == "Offline course draft", "a second SQLite connection reads the persisted offline edit");
        }
        StateLabel.Text = await sync.SynchronizeAsync();
        Show();
        LessonChecks.That(repository.Read().OperationId is not null && repository.Read().ConflictVersion == 2,
            "the real HTTP conflict preserves the local edit and records the server revision");
        await NativeCapture.SaveAsync(Path.Combine(Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!, "native-conflict.png"));
        if (mode == "problem")
            LessonChecks.That(repository.Read().OperationId is null, "incorrect expectation: a conflict silently discards the offline edit");
        AcceptServer(this,EventArgs.Empty);
        LessonChecks.That(repository.Read() is {Title:"Server revision",Version:2,OperationId:null}, "the explicit server-wins resolution updates SQLite atomically");
        TitleEntry.Text = "Approved revision";
        SaveOffline(this,EventArgs.Empty);
        var pending = repository.Read();
        StateLabel.Text = await sync.SynchronizeAsync();
        Show();
        LessonChecks.That(repository.Read() is {Version:3,OperationId:null}, "a version-matching edit synchronizes and clears its pending marker");
        var repeatedVersion = await sync.RepeatAsync(new Change(pending.OperationId!,pending.Version,pending.Title));
        LessonChecks.That(repeatedVersion == 3, "replaying the same operation identifier returns its original result");
    }
}
