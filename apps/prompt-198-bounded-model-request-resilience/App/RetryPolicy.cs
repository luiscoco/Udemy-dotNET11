using Microsoft.Extensions.AI;
using System.Net;
public sealed class RateLimitedException(TimeSpan delay)
    : HttpRequestException("Controlled provider-boundary HTTP 429", null, HttpStatusCode.TooManyRequests)
{
    public TimeSpan RetryAfter { get; } = delay;
}
public static class RetryPolicy
{
    public static async Task<ChatResponse> Respond(IChatClient chat, int maxAttempts, CancellationToken token)
    {
        for (int attempt = 1; ; attempt++)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                return await chat.GetResponseAsync("Explain bounded retries in one sentence.", AiRuntime.Options(), token);
            }
            catch (RateLimitedException e) when (attempt < maxAttempts)
            {
                var boundedDelay = TimeSpan.FromMilliseconds(Math.Clamp(e.RetryAfter.TotalMilliseconds, 0, 100));
                await Task.Delay(boundedDelay, token);
            }
        }
    }
}
public sealed class FaultClient(IChatClient inner, bool alwaysLimit = false, bool slow = false)
    : DelegatingChatClient(inner)
{
    public int Attempts { get; private set; }
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Attempts++;
        if (slow) await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        if (alwaysLimit || Attempts <= 2)
            throw new RateLimitedException(TimeSpan.FromMilliseconds(10));
        return await base.GetResponseAsync(messages, options, cancellationToken);
    }
}
