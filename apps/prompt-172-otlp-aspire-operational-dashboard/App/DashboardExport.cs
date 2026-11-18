using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
public static class DashboardExport
{
    public static void Send(string endpoint, bool broken)
    {
        var resource=ResourceBuilder.CreateDefault().AddService("course-dashboard");
        string prefix=endpoint+(broken?"/incorrect":"");
        using var traces=Sdk.CreateTracerProviderBuilder().SetResourceBuilder(resource).AddSource("Course.Dashboard")
            .AddOtlpExporter(options=>{options.Endpoint=new Uri(prefix+"/v1/traces");options.Protocol=OtlpExportProtocol.HttpProtobuf;}).Build();
        using var metrics=Sdk.CreateMeterProviderBuilder().SetResourceBuilder(resource).AddMeter("Course.Dashboard")
            .AddOtlpExporter(options=>{options.Endpoint=new Uri(prefix+"/v1/metrics");options.Protocol=OtlpExportProtocol.HttpProtobuf;}).Build();
        using var logs=LoggerFactory.Create(builder=>builder.AddOpenTelemetry(options=>
        {
            options.IncludeFormattedMessage=true; options.SetResourceBuilder(resource);
            options.AddOtlpExporter(export=>{export.Endpoint=new Uri(prefix+"/v1/logs");export.Protocol=OtlpExportProtocol.HttpProtobuf;});
        }));
        using var source=new ActivitySource("Course.Dashboard");
        using var meter=new Meter("Course.Dashboard");
        var requests=meter.CreateCounter<long>("course.requests");
        var duration=meter.CreateHistogram<double>("course.request.duration","ms");
        var logger=logs.CreateLogger("Course.Operations");
        for(int i=0;i<5;i++)
        {
            using var span=source.StartActivity("GET /courses",ActivityKind.Server);
            bool failed=i==4;
            span?.SetTag("http.response.status_code",failed?503:200);
            if(failed) span?.SetStatus(ActivityStatusCode.Error,"Synthetic catalog outage");
            requests.Add(1,new KeyValuePair<string,object?>("outcome",failed?"error":"success"));
            duration.Record(10+i,new KeyValuePair<string,object?>("route","/courses"));
            logger.LogInformation("Course request {RequestNumber} returned {StatusCode}",i+1,failed?503:200);
        }
        traces.ForceFlush(); metrics.ForceFlush();
    }
}
