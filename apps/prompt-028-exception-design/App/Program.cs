if (args.FirstOrDefault() == "bug")
{
    decimal[] receipts = [];
    Console.WriteLine(receipts[0]);
    return;
}

Check.That(ImportInput.TryReadNet("128.90", out var net) && net == 128.90m, "valid import amount parsed");
Check.That(!ImportInput.TryReadNet("invalid", out _), "expected input rejection is represented by a result");
Check.That(!ImportInput.TryReadNet(null, out _), "missing import amount is handled");
try
{
    ImportService.Load();
    throw new InvalidOperationException("Storage failure was swallowed");
}
catch (IOException error)
{
    Console.WriteLine(error.StackTrace);
    Check.That(error.StackTrace?.Contains("InvoiceStore.Load", StringComparison.Ordinal) == true,
        "origin frame preserved");
}
