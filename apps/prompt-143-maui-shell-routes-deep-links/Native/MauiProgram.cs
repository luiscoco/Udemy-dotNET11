using Microsoft.Extensions.DependencyInjection;
namespace NativeCourse;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<CourseSelection>();
        builder.Services.AddSingleton<CourseNavigation>();
        builder.Services.AddTransient<DetailsPage>();
        return builder.Build();
    }
}
