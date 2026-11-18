using Microsoft.Agents.AI;
public static class CancellationProbe
{
    public static async Task<bool> Verify(AIAgent agent,bool ignoreCancellation)
    {
        using var stop = new CancellationTokenSource();
        using var outer = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(stop.Token,outer.Token);
        var suppliedToken = ignoreCancellation ? outer.Token : linked.Token;
        try
        {
            await foreach (var update in agent.RunStreamingAsync(
                "Explain streaming in three sentences. Do not call any tool.",cancellationToken:suppliedToken))
            {
                suppliedToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(update.Text)) stop.Cancel();
            }
            suppliedToken.ThrowIfCancellationRequested();
            return false;
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested && !outer.IsCancellationRequested)
        { return true; }
    }
}
