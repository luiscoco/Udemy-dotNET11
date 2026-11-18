public static class NativePublish
{
    public static void Configure()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while(directory is not null)
        {
            string tools=Path.Combine(directory.FullName,".tools","aot-toolchain");
            if(Directory.Exists(tools))
            {
                string msvc=Directory.GetDirectories(Path.Combine(tools,"msvc","Contents","VC","Tools","MSVC")).Single();
                string sdk=Path.Combine(tools,"microsoft.windows.sdk.cpp.x64","c");
                Environment.SetEnvironmentVariable("PATH",Path.Combine(msvc,"bin","Hostx64","x64")+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH"));
                Environment.SetEnvironmentVariable("LIB",string.Join(Path.PathSeparator,new[]{Path.Combine(msvc,"lib","x64"),Path.Combine(sdk,"um","x64"),Path.Combine(sdk,"ucrt","x64")}));
                Environment.SetEnvironmentVariable("COURSE_ENVIRONMENTAL_LINKER","true");
                return;
            }
            directory=directory.Parent;
        }
    }
    public static string[] Arguments(string project,string folder,bool aot,bool readyToRun=false)
    {
        var args=new List<string>{"publish",project,"-c","Release","-r","win-x64","--self-contained","true",
            "-p:PublishAot="+aot.ToString().ToLowerInvariant(),"-p:PublishReadyToRun="+readyToRun.ToString().ToLowerInvariant(),"-o",folder};
        if(Environment.GetEnvironmentVariable("COURSE_ENVIRONMENTAL_LINKER")=="true") args.Add("-p:IlcUseEnvironmentalTools=true");
        return args.ToArray();
    }
}
