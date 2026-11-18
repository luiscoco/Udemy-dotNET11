using System.Security.Cryptography;

public static class Jobs
{
    public static int Worker(string file, string mode)
    {
        var store = new Store(file, true);
        using var c = store.Open();
        using (var claim = c.CreateCommand())
        {
            claim.CommandText = "UPDATE Jobs SET State='Running',Attempts=Attempts+1,Lease=$lease WHERE Id=1 AND State='Pending' AND Attempts<6";
            claim.Parameters.AddWithValue("$lease", DateTimeOffset.UtcNow.AddMilliseconds(500).ToUnixTimeMilliseconds());
            if (claim.ExecuteNonQuery() != 1)
                return 0;
        }

        if (mode == "crash-after-claim")
            return 17;
        string directory = Path.GetDirectoryName(file)!, input = Path.Combine(directory, "input.txt"), output = Path.Combine(directory, "output.txt");
        try
        {
            using var cancellation = new CancellationTokenSource();
            if (mode == "cancel")
                cancellation.Cancel();
            if (mode == "transient")
                throw new IOException("owned transient storage fixture");
            if (!File.Exists(output))
            {
                string temp = output + ".tmp";
                using (var source = File.OpenRead(input))
                using (var destination = File.Create(temp))
                    source.CopyToAsync(destination, cancellation.Token).GetAwaiter().GetResult();
                File.Move(temp, output, true);
            }

            if (mode == "crash-after-output")
                return 18;
            string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(output)));
            if (hash != Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))))
                throw new IOException("output integrity mismatch");
            using var tx = c.BeginTransaction();
            using var done = c.CreateCommand();
            done.Transaction = tx;
            done.CommandText = "INSERT OR IGNORE INTO Effects VALUES(1,$hash); UPDATE Jobs SET State='Completed',Lease=0 WHERE Id=1";
            done.Parameters.AddWithValue("$hash", hash);
            done.ExecuteNonQuery();
            tx.Commit();
            return 0;
        }
        catch (OperationCanceledException)
        {
            store.Execute("UPDATE Jobs SET State='Pending',Cancelled=Cancelled+1 WHERE Id=1");
            return 0;
        }
        catch (IOException)
        {
            store.Execute("UPDATE Jobs SET State='Pending',Retries=Retries+1 WHERE Id=1");
            return 0;
        }
    }

    public static int Recover(Store store) => store.Execute("UPDATE Jobs SET State='Pending',Lease=0 WHERE State='Running' AND Lease<$now", ("$now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
}
