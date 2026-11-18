using ModelContextProtocol.Client;
public static class CancellationCheck
{
    public static async Task<(bool ClientCancelled,bool ServerCancelled,bool Completed)> Run(McpClient client,string folder)
    {
        using var cancel=new CancellationTokenSource();
        var pending=client.CallToolAsync("slow_probe",McpSession.Args(("milliseconds",2000)),cancellationToken:cancel.Token);
        bool started=await McpSession.Until(()=>File.Exists(Path.Combine(folder,"started.txt")),TimeSpan.FromSeconds(10));
        Check.That(started,"the cancellation probe waits for an actual server-start signal");
        cancel.Cancel();bool clientCancelled=false;
        try { await pending; }catch(OperationCanceledException)when(cancel.IsCancellationRequested){clientCancelled=true;}
        await McpSession.Until(()=>File.Exists(Path.Combine(folder,"cancelled.txt"))||File.Exists(Path.Combine(folder,"completed.txt")),TimeSpan.FromSeconds(6));
        bool serverCancelled=File.Exists(Path.Combine(folder,"cancelled.txt")),completed=File.Exists(Path.Combine(folder,"completed.txt"));
        Console.WriteLine($"CANCEL client={clientCancelled} server={serverCancelled} completed={completed}");
        return (clientCancelled,serverCancelled,completed);
    }
}
