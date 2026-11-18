using System.Reflection;
using System.Runtime.InteropServices;
public sealed class OwnedComDictionary:IDisposable
{
    private object? instance;
    private readonly Type type;
    private readonly int ownerThread=Environment.CurrentManagedThreadId;
    public OwnedComDictionary()
    {
        if(Thread.CurrentThread.GetApartmentState()!=ApartmentState.STA)throw new InvalidOperationException("Use an STA worker");
        type=Type.GetTypeFromProgID("Scripting.Dictionary",throwOnError:true)!;instance=Activator.CreateInstance(type)!;
    }
    public Guid ClassId=>type.GUID;
    public bool IsDisposed=>instance is null;
    public void TextComparison()=>Call("CompareMode",BindingFlags.SetProperty,1);
    public void Add(string key,int value)=>Call("Add",BindingFlags.InvokeMethod,key,value);
    public bool Exists(string key)=>(bool)Call("Exists",BindingFlags.InvokeMethod,key)!;
    public int Count=>Convert.ToInt32(Call("Count",BindingFlags.GetProperty));
    private object? Call(string member,BindingFlags flags,params object[] args)
    {
        if(instance is null)throw new ObjectDisposedException(nameof(OwnedComDictionary));
        if(Environment.CurrentManagedThreadId!=ownerThread)throw new InvalidOperationException("COM object remains on its owner thread");
        return type.InvokeMember(member,flags,null,instance,args);
    }
    public void Dispose()
    {
        if(instance is null)return;
        if(Environment.CurrentManagedThreadId!=ownerThread)throw new InvalidOperationException("Release on owner thread");
        Marshal.FinalReleaseComObject(instance);instance=null;
    }
}
