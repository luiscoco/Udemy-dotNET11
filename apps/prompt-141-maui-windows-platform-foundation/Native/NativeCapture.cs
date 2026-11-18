using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
namespace NativeCourse;
public static class NativeCapture
{
    public static async Task SaveAsync(string file)
    {
        var window = (Microsoft.UI.Xaml.Window)Application.Current!.Windows[0].Handler!.PlatformView!;
        var bitmap = new RenderTargetBitmap();
        await bitmap.RenderAsync(window.Content);
        var pixels = await bitmap.GetPixelsAsync();
        using var stream = File.Open(file, FileMode.Create, FileAccess.ReadWrite);
        using var destination = stream.AsRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, destination);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
            (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
        await encoder.FlushAsync();
    }
}
