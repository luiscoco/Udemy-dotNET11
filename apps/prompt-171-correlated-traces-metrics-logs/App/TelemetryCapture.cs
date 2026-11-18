using System.Collections.Concurrent;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
public sealed class SpanCapture : BaseExporter<Activity>
{
    public ConcurrentQueue<object> Records { get; } = new();
    public ConcurrentQueue<Activity> Spans { get; } = new();
    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach(var span in batch)
        {
            Spans.Enqueue(span);
            Records.Enqueue(new { name=span.DisplayName, traceId=span.TraceId.ToString(), spanId=span.SpanId.ToString(), parentSpanId=span.ParentSpanId.ToString(), kind=span.Kind.ToString() });
        }
        return ExportResult.Success;
    }
}
public sealed class LogCapture : BaseExporter<LogRecord>
{
    public ConcurrentQueue<(string Message,string TraceId)> Records { get; } = new();
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        foreach(var log in batch) Records.Enqueue((log.FormattedMessage??log.Body?.ToString()??"",log.TraceId.ToString()));
        return ExportResult.Success;
    }
}
public sealed class MetricCapture : BaseExporter<Metric>
{
    public long Enrollments;
    public override ExportResult Export(in Batch<Metric> batch)
    {
        foreach(var metric in batch)
            if(metric.Name=="course.enrollments")
                foreach(ref readonly var point in metric.GetMetricPoints()) Enrollments=point.GetSumLong();
        return ExportResult.Success;
    }
}
