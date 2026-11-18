using System.Diagnostics;
using System.Runtime;
public static class PoolExperiment
{
    public static async Task<object> Run(bool asynchronous)
    {
        ThreadPool.GetMinThreads(out _,out int ioMin);
        ThreadPool.GetMaxThreads(out _,out int ioMax);
        Check.That(ThreadPool.SetMinThreads(4,ioMin),"configured fixture minimum");
        Check.That(ThreadPool.SetMaxThreads(4,ioMax),"configured fixture maximum");
        using var entered = new CountdownEvent(4);
        using var release = new ManualResetEventSlim();
        var asyncRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task[] occupied = Enumerable.Range(0,4).Select(_=>Task.Run(async ()=>
        {
            entered.Signal();
            if(asynchronous) await asyncRelease.Task;
            else release.Wait();
        })).ToArray();
        Check.That(entered.Wait(TimeSpan.FromSeconds(10)),"all four operations entered");
        var clock=Stopwatch.StartNew();
        var probe=Task.Run(()=>clock.Elapsed.TotalMilliseconds);
        Thread.Sleep(300);
        long queued=ThreadPool.PendingWorkItemCount;
        bool completedBeforeRelease=probe.IsCompleted;
        int threads=ThreadPool.ThreadCount;
        release.Set(); asyncRelease.SetResult();
        await Task.WhenAll(occupied).WaitAsync(TimeSpan.FromSeconds(10));
        double probeMs=await probe;
        int gen0=GC.CollectionCount(0),gen2=GC.CollectionCount(2);
        long allocated=GC.GetTotalAllocatedBytes(true);
        for(int i=0;i<8000;i++) GC.KeepAlive(new byte[4096]);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        return new { asynchronous, queued, completedBeforeRelease, threads, probeMs,
            serverGc=GCSettings.IsServerGC, latencyMode=GCSettings.LatencyMode.ToString(),
            allocatedBytes=GC.GetTotalAllocatedBytes(true)-allocated,
            gen0Collections=GC.CollectionCount(0)-gen0,gen2Collections=GC.CollectionCount(2)-gen2,
            pausePercentage=GC.GetGCMemoryInfo().PauseTimePercentage };
    }
}
