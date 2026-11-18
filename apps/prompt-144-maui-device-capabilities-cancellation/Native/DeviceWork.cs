using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
namespace NativeCourse;
public sealed class DeviceWork
{
    public async Task<string> InspectAsync()
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        var display = DeviceDisplay.Current.MainDisplayInfo;
        return $"Platform {DeviceInfo.Current.Platform}\nDisplay {display.Width} × {display.Height}\nConnectivity {Connectivity.Current.NetworkAccess}\nCamera permission status {permission}";
    }
    public async Task WriteOwnedFileAsync(string file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await File.WriteAllTextAsync(file, "Synthetic course note", cancellationToken);
    }
    public string CreateOwnedPath()
    {
        var folder = Path.Combine(FileSystem.Current.AppDataDirectory, "course-144", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "note.txt");
    }
}
