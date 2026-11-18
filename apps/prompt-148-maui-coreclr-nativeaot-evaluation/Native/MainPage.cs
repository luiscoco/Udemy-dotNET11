using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace NativeCourse;
public sealed class MainPage : ContentPage, ILessonPage
{
    private readonly Label result = new() { FontSize=22 };
    public MainPage()
    {
        BackgroundColor=Color.FromArgb("#F4F2FB");
        var inspect=new Button { Text="Inspect runtime",HorizontalOptions=LayoutOptions.Start };
        inspect.Clicked+=(_,_)=>Inspect();
        Content=new VerticalStackLayout
        {
            Padding=36,Spacing=20,
            Children={
                new Label{Text=".NET 11 · Runtime publishing",FontSize=18,TextColor=Color.FromArgb("#6642BB")},
                new Label{Text="Observe the executable you published",FontSize=30,FontAttributes=FontAttributes.Bold},
                inspect,new Border{Padding=24,BackgroundColor=Color.FromArgb("#E1F7EE"),Content=result},
                new Label{Text="The Windows target can be evaluated as a CoreCLR build and as a separately published Native AOT executable.",FontSize=18}
            }
        };
        Inspect();
    }
    private void Inspect()
    {
        result.Text=$"Runtime {RuntimeInformation.FrameworkDescription}\nDynamic code supported {RuntimeFeature.IsDynamicCodeSupported}\nDynamic code compiled {RuntimeFeature.IsDynamicCodeCompiled}\nArchitecture {RuntimeInformation.ProcessArchitecture}";
    }
    public async Task VerifyAsync(string mode)
    {
        Inspect();
        var expectedAot=mode=="aot";
        LessonChecks.That(RuntimeFeature.IsDynamicCodeSupported!=expectedAot,
            expectedAot?"the published Native AOT executable reports no dynamic code support":"the actual CoreCLR executable reports dynamic code support");
        LessonChecks.That(RuntimeFeature.IsDynamicCodeCompiled!=expectedAot,
            expectedAot?"the Native AOT executable has no JIT compilation":"the selected Windows CoreCLR runtime supports compiled dynamic code");
        if(mode=="problem")
            LessonChecks.That(!RuntimeFeature.IsDynamicCodeSupported,"incorrect expectation: the CoreCLR build is already Native AOT");
        await NativeCapture.SaveAsync(Path.Combine(Environment.GetEnvironmentVariable("COURSE_ARTIFACTS")!,expectedAot?"native-aot.png":"native-coreclr.png"));
    }
}
