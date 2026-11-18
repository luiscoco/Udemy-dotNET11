using System.Reflection;
using System.Runtime.Loader;
using Course.Contracts;
public sealed class PluginContext(string path):AssemblyLoadContext(isCollectible:true)
{
    private readonly AssemblyDependencyResolver resolver=new(path);
    protected override Assembly? Load(AssemblyName name)
    {
        if(name.Name==typeof(IPlugin).Assembly.GetName().Name)return typeof(IPlugin).Assembly;
        string? dependency=resolver.ResolveAssemblyToPath(name);
        return dependency is null?null:LoadFromAssemblyPath(dependency);
    }
}
