using InvoiceSuite.Core;
if (InvoiceMath.Total(128.90m) != 154.68m) throw new Exception("Receipt check failed");
Console.WriteLine("PASS: selected checks use the required Core dependency");
