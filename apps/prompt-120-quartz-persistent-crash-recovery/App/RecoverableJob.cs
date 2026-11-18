using Quartz;
using Microsoft.Data.Sqlite;
[DisallowConcurrentExecution]
public sealed class RecoverableJob:IJob
{
    public async ValueTask Execute(IJobExecutionContext context,CancellationToken cancellationToken=default)
    {
        var folder=context.MergedJobDataMap.GetString("folder")!;
        if(!context.Recovering)
        {
            await File.WriteAllTextAsync(Path.Combine(folder,"started.txt"),"entered original execution",cancellationToken);
            await Task.Delay(Timeout.InfiniteTimeSpan,cancellationToken);return;
        }
        using var connection=new SqliteConnection($"Data Source={Path.Combine(folder,"effects.db")};Pooling=False");connection.Open();
        using var command=connection.CreateCommand();
        command.CommandText="CREATE TABLE IF NOT EXISTS Effects (JobId TEXT PRIMARY KEY); INSERT OR IGNORE INTO Effects VALUES ('reserve');";
        command.ExecuteNonQuery();
        await File.WriteAllTextAsync(Path.Combine(folder,"completed.txt"),"Recovering=True",cancellationToken);
        Console.WriteLine("service: recovered job committed its idempotent business effect");
    }
}
