using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
public sealed class SecurityLog(bool unsafeFormatting):ILoggerProvider
{
    public ConcurrentQueue<string> Lines {get;}=new();
    public ILogger CreateLogger(string categoryName)=>new Sink(this,unsafeFormatting);
    public void Dispose(){}
    private sealed class Sink(SecurityLog owner,bool unsafeFormatting):ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState:notnull=>null;
        public bool IsEnabled(LogLevel level)=>true;
        public void Log<TState>(LogLevel level,EventId eventId,TState state,Exception? error,Func<TState,Exception?,string> formatter)
        {
            if(unsafeFormatting){owner.Lines.Enqueue(formatter(state,error));return;}
            var fields=state as IEnumerable<KeyValuePair<string,object?>>??[];
            owner.Lines.Enqueue(string.Join("; ",fields.Where(p=>p.Key is "UserId" or "Outcome").Select(p=>$"{p.Key}={p.Value}")));
        }
    }
}
