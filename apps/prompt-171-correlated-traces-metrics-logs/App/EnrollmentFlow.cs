using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
public sealed record Envelope(string Course, string? TraceParent);
public sealed class EnrollmentFlow(ILogger logger, string catalogAddress, bool dropContext)
{
    public static ActivitySource Source { get; } = new("Course.Enrollment");
    public static Meter Meter { get; } = new("Course.Enrollment");
    public static Counter<long> Enrollments { get; } = Meter.CreateCounter<long>("course.enrollments");
    public Channel<Envelope> Messages { get; } = Channel.CreateUnbounded<Envelope>();
    public async Task EnrollAsync()
    {
        using var activity=Source.StartActivity("api.enroll");
        using(var database=Source.StartActivity("database.insert",ActivityKind.Client))
        {
            await using var connection=new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            using var command=connection.CreateCommand();
            command.CommandText="CREATE TABLE Enrollments(Title TEXT); INSERT INTO Enrollments VALUES(@title);";
            command.Parameters.AddWithValue("@title","Diagnostics");
            await command.ExecuteNonQueryAsync();
            database?.SetTag("db.system.name","sqlite");
        }
        using var client=new HttpClient();
        string catalog=await client.GetStringAsync(catalogAddress+"/catalog");
        Check.That(catalog=="Diagnostics","real HTTP catalog operation completed");
        using(var producer=Source.StartActivity("messaging.publish",ActivityKind.Producer))
            await Messages.Writer.WriteAsync(new("Diagnostics",dropContext?null:producer!.Id));
        Enrollments.Add(1,new KeyValuePair<string,object?>("operation","enroll"));
        logger.LogInformation("API enrollment completed");
    }
    public async Task ConsumeAsync()
    {
        var envelope=await Messages.Reader.ReadAsync();
        ActivityContext.TryParse(envelope.TraceParent,null,out var parent);
        using var consumer=Source.StartActivity("messaging.consume",ActivityKind.Consumer,parent);
        logger.LogInformation("Consumer processed {Course}",envelope.Course);
    }
}
