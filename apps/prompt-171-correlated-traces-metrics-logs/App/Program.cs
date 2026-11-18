using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
var spans=new SpanCapture(); var logs=new LogCapture(); var metrics=new MetricCapture();
var resource=ResourceBuilder.CreateDefault().AddService("course-enrollment");
using var tracing=Sdk.CreateTracerProviderBuilder().SetResourceBuilder(resource).AddSource("Course.Enrollment")
    .AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddProcessor(new SimpleActivityExportProcessor(spans)).Build();
using var metering=Sdk.CreateMeterProviderBuilder().SetResourceBuilder(resource).AddMeter("Course.Enrollment")
    .AddReader(new PeriodicExportingMetricReader(metrics,60000)).Build();
using var logging=LoggerFactory.Create(builder=>builder.AddOpenTelemetry(options=>
{
    options.IncludeFormattedMessage=true;
    options.SetResourceBuilder(resource);
    options.AddProcessor(new SimpleLogRecordExportProcessor(logs));
}));
await using var catalog=await ApiHarness.StartAsync(_=>{},app=>app.MapGet("/catalog",()=>"Diagnostics"));
var flow=new EnrollmentFlow(logging.CreateLogger("Enrollment"),catalog.App.Urls.Single(),args.FirstOrDefault()=="problem");
await using var api=await ApiHarness.StartAsync(_=>{},app=>app.MapGet("/enroll",async ()=>{await flow.EnrollAsync();return Results.Ok();}));
using var response=await api.Client.GetAsync("/enroll"); response.EnsureSuccessStatusCode();
await flow.ConsumeAsync(); tracing.ForceFlush(); metering.ForceFlush();
Activity root=spans.Spans.Single(span=>span.DisplayName=="api.enroll");
foreach(string name in new[]{"database.insert","messaging.publish","messaging.consume"})
{
    var span=spans.Spans.Single(span=>span.DisplayName==name);
    Console.WriteLine($"{name} trace={span.TraceId} parent={span.ParentSpanId}");
}
Check.That(spans.Spans.Any(span=>span.Source.Name=="System.Net.Http"&&span.TraceId==root.TraceId),"outgoing HTTP span shares the enrollment trace");
Check.That(metrics.Enrollments==1,"metric exporter records one enrollment");
Directory.CreateDirectory("artifacts");
File.WriteAllText("artifacts/telemetry.json",JsonSerializer.Serialize(new { service="course-enrollment", spans=spans.Records, logs=logs.Records.Select(log=>new { log.Message, log.TraceId }), enrollments=metrics.Enrollments },new JsonSerializerOptions {WriteIndented=true}));
Check.That(spans.Spans.Where(span=>span.Source.Name=="Course.Enrollment").All(span=>span.TraceId==root.TraceId),"CONTEXT_REGRESSION message consumer preserves the original trace");
Check.That(logs.Records.Count==2&&logs.Records.All(log=>log.TraceId==root.TraceId.ToString()),"API and consumer logs retain trace correlation");
