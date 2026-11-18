if (args.Length == 0 ||
    !int.TryParse(args[0], out int divisor) ||
    divisor == 0)
{
    Console.WriteLine("Use a nonzero integer divisor.");
    return;
}

Console.WriteLine(10 / divisor);
