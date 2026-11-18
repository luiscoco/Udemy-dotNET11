using Microsoft.Maui.Devices;
using System.Runtime.InteropServices;
namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    public MainPage() => InitializeComponent();
    private void Inspect(object? sender, EventArgs args)
    {
        Result.Text = $"Platform {DeviceInfo.Current.Platform}\nArchitecture {RuntimeInformation.ProcessArchitecture}\nRuntime {RuntimeInformation.FrameworkDescription}";
    }
    public Task VerifyAsync(string mode)
    {
        ((IButtonController)InspectButton).SendClicked();
        LessonChecks.That(DeviceInfo.Current.Platform == DevicePlatform.WinUI, "the actual MAUI app reports the Windows platform");
        LessonChecks.That(Result.Text.Contains("11.0"), "the visible result reports the selected .NET 11 runtime");
        if (mode == "problem")
            LessonChecks.That(DeviceInfo.Current.Platform == DevicePlatform.Android,
                "incorrect expectation: the selected Windows app runs on Android");
        return Task.CompletedTask;
    }
}
