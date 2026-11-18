using System.Diagnostics;
public static class Fixture
{
    public static (string Root, string Outside) Create()
    {
        string root = Path.GetFullPath("artifacts/imports");
        string outside = Path.GetFullPath("artifacts/imports-other");
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(root, "nested", "lesson.txt"), "course input");
        File.WriteAllText(Path.Combine(outside, "outside.txt"), "separate fixture");
        string link = Path.Combine(root, "linked");
        if (!Directory.Exists(link))
        {
            if (OperatingSystem.IsWindows())
            {
                var info = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (string argument in new[] { "/c", "mklink", "/J", link, outside }) info.ArgumentList.Add(argument);
                var result = Process.RunAndCaptureText(info);
                if (result.ExitStatus.ExitCode != 0) throw new IOException(result.StandardError);
            }
            else Directory.CreateSymbolicLink(link, outside);
        }
        return (root, outside);
    }
}
