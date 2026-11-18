public static class GameTool
{
    public static string Find()
    {
        string? supplied=Environment.GetEnvironmentVariable("COURSE_GODOT");if(!string.IsNullOrEmpty(supplied)&&File.Exists(supplied))return supplied;
        for(var dir=new DirectoryInfo(Directory.GetCurrentDirectory());dir!=null;dir=dir.Parent){string file=Path.Combine(dir.FullName,".tools/godot-4.7.2/Godot_v4.7.2-stable_win64_console.exe");if(File.Exists(file))return file;}
        throw new FileNotFoundException("Set COURSE_GODOT to the Godot 4.7.2 standard console executable.");
    }
}
