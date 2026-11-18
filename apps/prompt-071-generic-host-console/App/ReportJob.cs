using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
public interface IReportFormatter { string Format(string title, int number); }
public sealed class ReportFormatter : IReportFormatter
{
    public string Format(string title, int number) => $"{title} / copy {number}";
}
public sealed class ReportJob(IConfiguration config, IReportFormatter formatter, ILogger<ReportJob> logger)
{
    public string[] Run()
    {
        string title = config["Report:Title"] ?? throw new InvalidOperationException("Report title is required");
        int copies = config.GetValue<int>("Report:Copies");
        var results = Enumerable.Range(1, copies).Select(i => formatter.Format(title, i)).ToArray();
        logger.LogInformation(new EventId(101, "ReportCreated"), "Created {CopyCount} report copies", results.Length);
        return results;
    }
}
