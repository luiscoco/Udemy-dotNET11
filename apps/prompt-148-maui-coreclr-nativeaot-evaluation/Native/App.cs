using System.Text.Json;
namespace NativeCourse;
public sealed class App(MainPage page) : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window=new Window(page){Title="Lesson 148 · Runtime evaluation",Width=1100,Height=760};
        var mode=Environment.GetEnvironmentVariable("COURSE_MODE");
        if(!string.IsNullOrEmpty(mode)) page.Loaded+=async(_,_)=>await VerifyAsync(page,mode);
        return window;
    }
    private static async Task VerifyAsync(MainPage page,string mode)
    {
        var directory=Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!;
        var report=new NativeReport{mode=mode,checks=LessonChecks.Messages};
        try
        {
            await Task.Delay(350);
            await page.VerifyAsync(mode);
            await NativeCapture.SaveAsync(Path.Combine(directory,"native-success.png"));
            report.passed=true;
        }
        catch(Exception exception)
        {
            report.error=exception.Message;
            try{await NativeCapture.SaveAsync(Path.Combine(directory,"native-problem.png"));}catch{}
        }
        var json=JsonSerializer.Serialize(report,NativeJson.Default.NativeReport);
        await File.WriteAllTextAsync(Path.Combine(directory,"native-"+mode+".json"),json);
        if(report.passed) await File.WriteAllTextAsync(Path.Combine(directory,"browser-ui-verification.json"),json);
        Environment.Exit(report.passed?0:1);
    }
}
