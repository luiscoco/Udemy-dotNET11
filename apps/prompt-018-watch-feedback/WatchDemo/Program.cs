while (true)
{
    Console.WriteLine($"{MessageProvider.Text()} pid={Environment.ProcessId}");
    await Task.Delay(500);
}
