using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
var builder = Host.CreateApplicationBuilder();
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile(Path.GetFullPath("appsettings.json"), optional: false);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.SingleLine = true);
if (args.FirstOrDefault() != "missing") builder.Services.AddSingleton<IReportFormatter, ReportFormatter>();
builder.Services.AddTransient<ReportJob>();
using var host = builder.Build();
await host.StartAsync();
try
{
    var job = host.Services.GetRequiredService<ReportJob>();
    string[] rows = job.Run();
    foreach (string row in rows) Console.WriteLine(row);
    Check.That(rows.SequenceEqual(new[] { "Classroom report / copy 1", "Classroom report / copy 2" }),
        "JSON configuration, constructor injection and the report job agree");
    Check.That(host.Services.GetRequiredService<IConfiguration>()["Report:Copies"] == "2",
        "the host exposes the configured values through dependency injection");
}
finally { await host.StopAsync(); }
