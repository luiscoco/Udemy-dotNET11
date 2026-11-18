using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace NativeCourse;
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs args)
    {
        base.OnStartup(args);
        var window = new MainWindow(new CourseViewModel());
        window.Show();
        var mode = Environment.GetEnvironmentVariable("COURSE_MODE");
        if (string.IsNullOrEmpty(mode)) return;
        bool passed = false;
        string? error = null;
        try
        {
            await Task.Delay(200);
            window.Verify(mode);
            passed = true;
        }
        catch (Exception exception) { error = exception.Message; }
        var directory = Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        await Task.Delay(100);
        var dpi = VisualTreeHelper.GetDpi(window);
        var bitmap = new RenderTargetBitmap((int)(window.ActualWidth*dpi.DpiScaleX),(int)(window.ActualHeight*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = File.Create(Path.Combine(directory,passed?"native-success.png":"native-problem.png"))) encoder.Save(stream);
        var report = new {passed,mode,scenario="native-wpf-149",evidenceKind="Actual WPF controls and rendered visual tree",checks=window.Checks,error};
        var json = JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});
        await File.WriteAllTextAsync(Path.Combine(directory,"native-"+mode+".json"),json);
        if(passed) await File.WriteAllTextAsync(Path.Combine(directory,"browser-ui-verification.json"),json);
        Shutdown(passed?0:1);
    }
}
