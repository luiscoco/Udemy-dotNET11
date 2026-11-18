using Microsoft.Extensions.DependencyInjection;
namespace NativeCourse;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
