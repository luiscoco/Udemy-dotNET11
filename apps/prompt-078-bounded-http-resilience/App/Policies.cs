using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
public static class Policies
{
    public static bool Transient(Exception? error) => error is TimeoutRejectedException ||
        error is HttpRequestException http && (http.StatusCode is null || (int)http.StatusCode >= 500 ||
            http.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout);
    public static ResiliencePipeline<int> Build(RetryBudget budget, int minimumThroughput = 100, TimeSpan? attemptTimeout = null)
    {
        return new ResiliencePipelineBuilder<int>()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddRetry(new RetryStrategyOptions<int>
            {
                MaxRetryAttempts = 2, Delay = TimeSpan.FromMilliseconds(5), UseJitter = false,
                ShouldHandle = args => new ValueTask<bool>(args.AttemptNumber < 2 && Transient(args.Outcome.Exception) && budget.TrySpend()),
                OnRetry = args => { Console.WriteLine($"RETRY {args.AttemptNumber + 1}; shared tokens used {budget.Used}"); return default; }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<int>
            {
                FailureRatio = 1, MinimumThroughput = minimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(10), BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = args => new ValueTask<bool>(Transient(args.Outcome.Exception))
            })
            .AddTimeout(attemptTimeout ?? TimeSpan.FromSeconds(2)).Build();
    }
    public static ValueTask<int> GetAsync(ResiliencePipeline<int> pipeline, HttpClient client, string address) =>
        pipeline.ExecuteAsync(async token =>
        {
            using var response = await client.GetAsync(address, token);
            response.EnsureSuccessStatusCode();
            return (int)response.StatusCode;
        });
}
