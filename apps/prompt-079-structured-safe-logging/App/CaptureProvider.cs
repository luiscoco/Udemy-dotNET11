using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
public sealed record CapturedEvent(int EventId, string Level, string Category, Dictionary<string, object?> Fields);
public sealed class CaptureProvider : ILoggerProvider, ISupportExternalScope
{
    public ConcurrentQueue<CapturedEvent> Events { get; } = new();
    private IExternalScopeProvider scopes = new LoggerExternalScopeProvider();
    public void SetScopeProvider(IExternalScopeProvider provider) => scopes = provider;
    public ILogger CreateLogger(string category) => new Sink(this, category);
    public void Dispose() { }
    private sealed class Sink(CaptureProvider owner, string category) : ILogger
    {
        public bool IsEnabled(LogLevel level) => level >= LogLevel.Information;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => owner.scopes.Push(state);
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(level)) return;
            var fields = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, object?> { ["Message"] = state };
            owner.scopes.ForEachScope((scope, target) =>
            {
                if (scope is IEnumerable<KeyValuePair<string, object?>> entries)
                    foreach (var entry in entries) target["Scope." + entry.Key] = entry.Value;
            }, fields);
            owner.Events.Enqueue(new(id.Id, level.ToString(), category, fields));
        }
    }
}
