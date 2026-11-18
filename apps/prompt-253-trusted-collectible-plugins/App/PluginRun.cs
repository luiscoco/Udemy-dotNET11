using System.Runtime.CompilerServices;
using Course.Contracts;
public static class PluginRun
{
    private static IPlugin? pinned;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WeakReference Execute(string path,bool keepRoot,out string result)
    {
        var context=new PluginContext(path);var assembly=context.LoadFromAssemblyPath(path);
        var plugin=(IPlugin)Activator.CreateInstance(assembly.GetType("Course.Plugin.Greeting")!)!;
        result=plugin.Execute("student");
        if(keepRoot)pinned=plugin;
        var weak=new WeakReference(context,trackResurrection:true);context.Unload();return weak;
    }
    public static bool Collected(WeakReference weak)
    {
        for(int i=0;i<12&&weak.IsAlive;i++){GC.Collect();GC.WaitForPendingFinalizers();}
        return !weak.IsAlive;
    }
    public static void ClearRoot()=>pinned=null;
}
