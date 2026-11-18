using Polly.CircuitBreaker;
using Polly.Timeout;
await using var peer = new Peer(); await peer.StartAsync();
using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
var budget = new RetryBudget(2);
var pipeline = Policies.Build(budget);
Check.That(await Policies.GetAsync(pipeline, client, peer.Address + "/flaky") == 200 && peer.FlakyCalls == 3,
    "two bounded retries recover the third loopback attempt");
int before = peer.FailingCalls;
try { _ = await Policies.GetAsync(pipeline, client, peer.Address + "/failing"); }
catch (HttpRequestException) { Console.WriteLine("Observed the failed request after retry budget exhaustion"); }
int attempts = peer.FailingCalls - before;
Console.WriteLine($"Shared retry tokens used {budget.Used}; exhausted-budget attempts {attempts}");
if (args.FirstOrDefault() == "budget") Check.That(attempts == 3, "an exhausted shared budget cannot fund another two retries");
Check.That(attempts == 1 && budget.Used == 2, "the shared budget limits amplification across logical requests");
var breaker = Policies.Build(new RetryBudget(0), minimumThroughput: 2);
for (int i = 0; i < 2; i++)
    try { _ = await Policies.GetAsync(breaker, client, peer.Address + "/failing"); } catch (HttpRequestException) { }
before = peer.FailingCalls;
if (args.FirstOrDefault() == "circuit") { _ = await Policies.GetAsync(breaker, client, peer.Address + "/failing"); return; }
bool open = false;
try { _ = await Policies.GetAsync(breaker, client, peer.Address + "/failing"); }
catch (BrokenCircuitException) { open = true; Console.WriteLine("CIRCUIT OPEN: request rejected before HTTP execution"); }
Check.That(open && peer.FailingCalls == before, "an open circuit prevents another peer request");
var timed = Policies.Build(new RetryBudget(0), attemptTimeout: TimeSpan.FromMilliseconds(200));
if (args.FirstOrDefault() == "timeout") { _ = await Policies.GetAsync(timed, client, peer.Address + "/slow"); return; }
bool expired = false;
try { _ = await Policies.GetAsync(timed, client, peer.Address + "/slow"); }
catch (TimeoutRejectedException) { expired = true; Console.WriteLine("TIMEOUT: the cooperative HTTP attempt was cancelled"); }
await peer.SlowExited.Task.WaitAsync(TimeSpan.FromSeconds(5));
Check.That(expired, "the attempt timeout is observed and the peer handler exits");
