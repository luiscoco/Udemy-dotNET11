using System.Diagnostics;
using System.Security.Cryptography;
Directory.CreateDirectory("artifacts");
string path = "artifacts/payload.bin";
byte[] payload = Enumerable.Range(0, 1024 * 1024).Select(i => (byte)i).ToArray();
await File.WriteAllBytesAsync(path, payload);
string actual = await Work.ReadHashAsync(path, CancellationToken.None);
Check.That(actual == Convert.ToHexString(SHA256.HashData(payload)), "native asynchronous file path preserves the expected digest");
if (args.FirstOrDefault() == "cancel")
{
    using var source = new CancellationTokenSource();
    source.Cancel();
    _ = await Task.Run(() => Work.CountPrimes(100_000, source.Token));
    return;
}
var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
Task<int> computation = Task.Run(async () =>
{
    await gate.Task;
    return Work.CountPrimes(100_000, CancellationToken.None);
});
if (args.FirstOrDefault() == "unawaited")
{
    bool ready = computation.IsCompleted;
    gate.SetResult();
    _ = await computation;
    Check.That(ready, "scheduling work does not mean its result was already available");
    return;
}
Check.That(!computation.IsCompleted, "the caller continues while the controlled CPU task is pending");
gate.SetResult();
int count = await computation;
Check.That(count == 9592, "awaiting the scheduled calculation produces the verified prime count");
Check.That(Work.CountPrimes(100_000, CancellationToken.None) == count, "direct and scheduled CPU calculations agree");
Console.WriteLine($"Primes through 100000: {count}; I/O digest {actual}");
Console.WriteLine("Task.Run is an offloading choice for a responsive caller, not extra server CPU capacity.");
