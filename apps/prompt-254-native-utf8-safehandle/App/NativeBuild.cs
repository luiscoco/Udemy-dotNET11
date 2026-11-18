using System.Runtime.InteropServices;
public static class NativeBuild
{
    public static async Task Compile()
    {
        var root=new DirectoryInfo(Environment.CurrentDirectory);string compiler="cl.exe";
        while(root!=null)
        {
            string tools=Path.Combine(root.FullName,".tools","aot-toolchain");
            if(Directory.Exists(tools))
            {
                string msvc=Directory.GetDirectories(Path.Combine(tools,"msvc","Contents","VC","Tools","MSVC")).Single();
                string headers=Directory.GetDirectories(Path.Combine(tools,"microsoft.windows.sdk.cpp","c","Include")).Single();
                string libs=Path.Combine(tools,"microsoft.windows.sdk.cpp.x64","c");
                compiler=Path.Combine(msvc,"bin","Hostx64","x64","cl.exe");
                Environment.SetEnvironmentVariable("PATH",Path.GetDirectoryName(compiler)+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH"));
                Environment.SetEnvironmentVariable("INCLUDE",string.Join(Path.PathSeparator,new[]{Path.Combine(msvc,"include"),Path.Combine(headers,"ucrt"),Path.Combine(headers,"shared"),Path.Combine(headers,"um")}));
                Environment.SetEnvironmentVariable("LIB",string.Join(Path.PathSeparator,new[]{Path.Combine(msvc,"lib","x64"),Path.Combine(libs,"um","x64"),Path.Combine(libs,"ucrt","x64")}));break;
            }
            root=root.Parent;
        }
        Directory.CreateDirectory("artifacts");
        var built=await Harness.Run(compiler,"/nologo","/LD","/O2","/W4","/WX","/MD","/utf-8","/Foartifacts/course_native.obj","Native/course_native.c","/link","/OUT:artifacts/course_native.dll","/IMPLIB:artifacts/course_native.lib");
        Console.WriteLine(built.Text);Check.That(built.Code==0,"the owned native C library compiles successfully");
        NativeLibrary.SetDllImportResolver(typeof(Native).Assembly,(name,assembly,path)=>name=="course_native"?NativeLibrary.Load(Path.GetFullPath("artifacts/course_native.dll")):IntPtr.Zero);
    }
}
