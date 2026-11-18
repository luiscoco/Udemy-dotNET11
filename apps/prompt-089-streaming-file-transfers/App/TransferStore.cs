using System.Collections.Concurrent;
public sealed record StoredFile(Guid Id,string Path,string DownloadName,long Length);
public sealed record FileReceipt(Guid Id,long Length);
public sealed class TransferStore
{
    public const int MaximumBytes=65_536;
    public string Root {get;}=Path.GetFullPath("artifacts/private-uploads");
    public ConcurrentDictionary<Guid,StoredFile> Files {get;}=new();
    public TaskCompletionSource Entered {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Cleaned {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TransferStore() { Directory.CreateDirectory(Root); }
}
