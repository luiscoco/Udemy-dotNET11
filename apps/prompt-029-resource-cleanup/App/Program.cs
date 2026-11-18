Directory.CreateDirectory("artifacts/receipts");
string path = Path.Combine("artifacts", "receipts", "receipt.txt");
var trace = new List<string>();

if (args.FirstOrDefault() == "held")
{
    using var first = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    using var second = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    return;
}

var synchronous = new ReceiptWriter(path, trace);
using (synchronous)
{
    await synchronous.WriteAsync("Receipt 154.68");
}
synchronous.Dispose();
Check.That(trace.Count == 1 && trace[0] == "sync closed", "synchronous cleanup is repeatable");

await using (var asynchronous = new ReceiptWriter(path, trace))
{
    await asynchronous.WriteAsync("Async receipt 154.68");
}
Check.That(trace.Contains("async closed"), "await using completes asynchronous cleanup");
Check.That(await File.ReadAllTextAsync(path) == "Async receipt 154.68", "receipt is readable after close");

try
{
    await using var exceptional = new ReceiptWriter(path, trace);
    await exceptional.WriteAsync("Before the controlled failure");
    throw new InvalidOperationException("Exercise failure");
}
catch (InvalidOperationException)
{
    Console.WriteLine("Observed controlled scope failure");
}
using var reopened = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
Check.That(trace.Count(item => item == "async closed") == 2, "exceptional scope also completed asynchronous cleanup");
Console.WriteLine(string.Join(" / ", trace));
