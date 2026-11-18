using Avalonia;
using Avalonia.Headless;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
if(args.Contains("--interactive")){AppBuilder.Configure<CourseApp>().UsePlatformDetect().StartWithClassicDesktopLifetime(args);return;}
bool broken=args.Contains("problem");
AppBuilder.Configure<CourseApp>().UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions{UseHeadlessDrawing=false}).SetupWithoutStarting();
var view=new QuoteWindow(broken);view.Show();Dispatcher.UIThread.RunJobs();
view.Quantity.Focus();view.Quantity.SelectAll();
view.KeyTextInput("3");Dispatcher.UIThread.RunJobs();
var center=view.Calculate.TranslatePoint(new Point(view.Calculate.Bounds.Width/2,view.Calculate.Bounds.Height/2),view)!.Value;
view.MouseDown(center,MouseButton.Left,RawInputModifiers.None);view.MouseUp(center,MouseButton.Left,RawInputModifiers.None);
Dispatcher.UIThread.RunJobs();
Directory.CreateDirectory("artifacts");
string image=Path.GetFullPath("artifacts/native-ui.png");
using(var bitmap=view.CaptureRenderedFrame()??throw new InvalidOperationException("No actual Skia frame"))bitmap.Save(image,Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
UiEvidence.Record(image,new{actualAvalonia=true,quantity=view.Quantity.Text,total=view.Total,broken,skiaRendered=true});
Console.WriteLine("DESKTOP actual keyboard input "+view.Quantity.Text+" actual mouse command result "+view.Total);
Check.That(view.Quantity.Text=="3"&&view.Total==37.50m,"EXT desktop command ignored quantity");
view.Close();
Harness.Save("ext-evidence.json",new{passed=true,actualAvalonia=true,keyboard=true,mouse=true,skiaRendered=true,quantity=3,total=view.Total});
Console.WriteLine("PASS real Avalonia controls, keyboard, mouse, command and rendered framebuffer");
public sealed class CourseApp:Application
{
    public override void Initialize()=>Styles.Add(new FluentTheme());
    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)desktop.MainWindow=new QuoteWindow(false);
        base.OnFrameworkInitializationCompleted();
    }
}

