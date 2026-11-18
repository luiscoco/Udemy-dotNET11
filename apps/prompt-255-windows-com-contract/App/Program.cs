using System.Runtime.ExceptionServices;
Exception? failure=null;object? evidence=null;
var thread=new Thread(()=>
{
    try
    {
        using var dictionary=new OwnedComDictionary();
        if(!args.Contains("problem"))dictionary.TextComparison();
        dictionary.Add("NET11",3);bool found=dictionary.Exists("net11");
        Console.WriteLine($"COM clsid={dictionary.ClassId} apartment={Thread.CurrentThread.GetApartmentState()} count={dictionary.Count} lowercaseFound={found}");
        Check.That(found&&dictionary.Count==1,"the actual COM component honors the configured text-comparison contract","EXT COM comparison contract was not configured");
        Guid clsid=dictionary.ClassId;dictionary.Dispose();
        Check.That(dictionary.IsDisposed,"the privately owned RCW is released on its creating STA thread");
        evidence=new {passed=true,actualCom=true,clsid,apartment="STA",count=1,found,disposed=dictionary.IsDisposed,platform=System.Runtime.InteropServices.RuntimeInformation.OSDescription};
    }
    catch(Exception e){failure=e;}
});
thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
if(failure!=null)ExceptionDispatchInfo.Capture(failure).Throw();
Harness.Save("ext-evidence.json",evidence!);
