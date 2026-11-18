using Microsoft.Extensions.AI;
bool broken = args.Contains("problem");
using var live = AiRuntime.Create();
var fixture = new FaultClient(live, alwaysLimit: broken);
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
ChatResponse? response = null;
try { response = await RetryPolicy.Respond(fixture, broken ? 5 : 3, deadline.Token); }
catch (RateLimitedException) { Console.WriteLine("RETRY controlled rate limit exhausted"); }
Console.WriteLine($"RETRY attempts={fixture.Attempts} budget=3 liveResponse={response is not null}");
AiRuntime.Evidence("retry-boundary.json", new { attempts = fixture.Attempts, liveResponse = response is not null });
Check.That(fixture.Attempts <= 3, "model request stays within three attempts", "AI retry budget exceeded");
Check.That(response?.Text.Length > 0, "two injected rate limits followed by actual model inference within budget");
Console.WriteLine("MODEL " + response!.Text.Replace('\n', ' '));
var slow = new FaultClient(live, slow: true);
using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));
bool timedOut = false;
try { await RetryPolicy.Respond(slow, 3, timeout.Token); }
catch (OperationCanceledException) { timedOut = true; }
Check.That(timedOut && slow.Attempts == 1, "deadline cancels the provider-boundary operation without retrying cancellation");
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
bool preCancelled = false;
try { await RetryPolicy.Respond(fixture, 3, cancelled.Token); }
catch (OperationCanceledException) { preCancelled = true; }
Check.That(preCancelled && fixture.Attempts == 3, "caller cancellation sends no extra request");
AiRuntime.Evidence("ai-evidence.json", new { passed = true, attempts = fixture.Attempts, timedOut, preCancelled,
    text = response.Text, fixture = "injected IChatClient faults, third attempt forwards to real Ollama inference" });
