using Quartz;
public static class SchedulerWorker
{
    public static async Task Run(string folder,bool memory,bool seed)
    {
        var scheduler=await QuartzSchedulerBuilder.Create(q=>
        {
            q.ConfigureScheduler(o=>{o.InstanceName="course120";o.InstanceId="NON_CLUSTERED";});
            q.UseDefaultThreadPool(maxConcurrency:1);
            if(memory)q.UseInMemoryStore();
            else q.UsePersistentStore(store=>
            {
                store.UseSqlite($"Data Source={Path.Combine(folder,"jobs.db")};Pooling=False");
                store.ProvisionSchema();
            });
        }).BuildScheduler();
        if(seed)
        {
            var job=JobBuilder.Create<RecoverableJob>().WithIdentity("reserve")
                .UsingJobData("folder",folder).RequestRecovery().Build();
            await scheduler.ScheduleJob(job,TriggerBuilder.Create().WithIdentity("reserve-now").StartNow().Build());
        }
        await scheduler.Start();
        if(seed)await Task.Delay(Timeout.InfiniteTimeSpan);
        else
        {
            var until=DateTime.UtcNow.AddSeconds(memory?1:15);
            while(!File.Exists(Path.Combine(folder,"completed.txt")) && DateTime.UtcNow<until)await Task.Delay(25);
            await scheduler.Shutdown(waitForJobsToComplete:true);
        }
    }
}
