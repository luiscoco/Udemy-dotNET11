using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Web.WebView2.Core;
namespace NativeCourse;
public partial class MainPage : ContentPage, ILessonPage
{
    private readonly RatingBridge bridge = new();
    private readonly TaskCompletionSource initialized = new();
    public MainPage()
    {
        InitializeComponent();
        Hybrid.SetInvokeJavaScriptTarget(bridge);
        Hybrid.WebViewInitialized += (_, _) => initialized.TrySetResult();
        bridge.Rated += result => MainThread.BeginInvokeOnMainThread(() => NativeResult.Text = result);
    }
    private async Task WaitForContentAsync()
    {
        await initialized.Task.WaitAsync(TimeSpan.FromSeconds(15));
        var view = (Microsoft.UI.Xaml.Controls.WebView2)Hybrid.Handler!.PlatformView!;
        var html = await view.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
        await File.WriteAllTextAsync(Path.Combine(Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!,"native-web-document.txt"),html);
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if ((await Hybrid.EvaluateJavaScriptAsync("window.bridgeReady || ''").WaitAsync(TimeSpan.FromSeconds(5)))?.Contains("ready") == true) return;
            }
            catch (Exception) when (attempt < 9) { }
            await Task.Delay(100);
        }
        throw new TimeoutException("The owned HybridWebView content did not initialize.");
    }
    public async Task VerifyAsync(string mode)
    {
        await WaitForContentAsync();
        var rejected = await Hybrid.InvokeJavaScriptAsync<string>("runRejected",BridgeJson.Default.String).WaitAsync(TimeSpan.FromSeconds(10));
        LessonChecks.That(rejected == "rejected" && bridge.LastRating == 0,
            "the actual JavaScript-to-native bridge rejects an out-of-range rating");
        if (mode == "problem")
            LessonChecks.That(bridge.LastRating == 99, "incorrect expectation: the web bridge accepts an out-of-range rating");
        var result = await Hybrid.InvokeJavaScriptAsync<string>("runAllowed",BridgeJson.Default.String).WaitAsync(TimeSpan.FromSeconds(10));
        LessonChecks.That(bridge.LastRating == 5 && result?.Contains("Recorded 5") == true, "local JavaScript invokes the narrow validated native method");
        LessonChecks.That(NativeResult.Text.Contains("Recorded 5"), "the normal bridge callback updates the native status control");
        var unknown = await Hybrid.InvokeJavaScriptAsync<string>("runUnknown",BridgeJson.Default.String).WaitAsync(TimeSpan.FromSeconds(10));
        LessonChecks.That(unknown == "rejected", "the native dispatcher rejects an operation outside its allowlist");
        await Hybrid.InvokeJavaScriptAsync<string>("runAllowed",BridgeJson.Default.String);
        await Task.Delay(100);
        var view = (Microsoft.UI.Xaml.Controls.WebView2)Hybrid.Handler!.PlatformView!;
        using var file = File.Open(Path.Combine(Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!, "native-web-content.png"),FileMode.Create,FileAccess.ReadWrite);
        using var stream = file.AsRandomAccessStream();
        await view.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,stream);
    }
}
