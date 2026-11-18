using System;
public interface IMoneyRules
{
    public static decimal AddTax(decimal net)
    {
        return Math.Round(net * 1.20m, 2);
    }
}
public static class LegacyProgram
{
    public static int Main()
    {
        decimal total = IMoneyRules.AddTax(128.90m);
        Console.WriteLine("Runtime " + Environment.Version);
        if (total != 154.68m) return 1;
        Console.WriteLine("PASS static interface helper on .NET Framework CLR 4");
        return 0;
    }
}
