using System.Security.Cryptography;
public static class Work
{
    public static async Task<string> ReadHashAsync(string path, CancellationToken token)
    {
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, 16384, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Convert.ToHexString(await SHA256.HashDataAsync(file, token));
    }
    public static int CountPrimes(int limit, CancellationToken token)
    {
        int total = 0;
        for (int number = 2; number <= limit; number++)
        {
            token.ThrowIfCancellationRequested();
            bool prime = true;
            for (int divisor = 2; divisor * divisor <= number; divisor++)
                if (number % divisor == 0) { prime = false; break; }
            if (prime) total++;
        }
        return total;
    }
}
