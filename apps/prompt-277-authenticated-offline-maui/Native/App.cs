using System.Text.Json;

namespace NativeCourse;
public sealed class App(MainPage page) : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(page)
        {
            Title = "Lesson 277 · Offline SQLite and explicit conflict resolution",
            Width = 1100,
            Height = 760
        };
        var mode = Environment.GetEnvironmentVariable("COURSE_MODE");
        window.HandlerChanged += (_, _) =>
        {
            if (!string.IsNullOrEmpty(mode) && window.Handler?.PlatformView is Microsoft.UI.Xaml.Window native)
                native.AppWindow.Move(new Windows.Graphics.PointInt32(-18000, -18000));
        };
        if (!string.IsNullOrEmpty(mode))
            page.Loaded += async (_, _) => await VerifyAsync(page, mode);
        return window;
    }

    private static async Task VerifyAsync(MainPage page, string mode)
    {
        var artifacts = Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!;
        bool passed = false;
        string? error = null;
        try
        {
            await Task.Delay(350);
            await ((ILessonPage)page).VerifyAsync(mode);
            await Task.Delay(150);
            await NativeCapture.SaveAsync(Path.Combine(artifacts, "native-success.png"));
            passed = true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            await Task.Delay(150); // Allow native layout to draw the error state before capture.
            try
            {
                await NativeCapture.SaveAsync(Path.Combine(artifacts, "native-problem.png"));
            }
            catch
            {
            }
        }

        var result = new
        {
            passed,
            mode,
            scenario = "native-maui-277",
            evidenceKind = "Actual native application self-verification and WinUI-rendered controls",
            checks = LessonChecks.Messages,
            error
        };
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        await File.WriteAllTextAsync(Path.Combine(artifacts, "native-" + mode + ".json"), JsonSerializer.Serialize(result, options));
        if (passed)
            await File.WriteAllTextAsync(Path.Combine(artifacts, "browser-ui-verification.json"), JsonSerializer.Serialize(result, options));
        Environment.Exit(passed ? 0 : 1);
    }
}
