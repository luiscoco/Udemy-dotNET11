using Microsoft.Extensions.Options;
public sealed class EndpointOptions
{
    public string MarkerPath { get; set; } = "";
}
public sealed class ValidationProbe(bool wait)
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool SawCancellation;
    public async Task<bool> CheckAsync(EndpointOptions options, CancellationToken token)
    {
        Entered.TrySetResult();
        try
        {
            if (wait) await Task.Delay(10_000, token); // Controlled cancellable validation wait.
            string marker = await File.ReadAllTextAsync(options.MarkerPath, token);
            return marker == "READY";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            SawCancellation = true;
            Console.WriteLine("VALIDATION cancellation reached the asynchronous rule");
            throw;
        }
    }
}

public sealed class MarkerValidator(ValidationProbe probe) : IAsyncValidateOptions<EndpointOptions>
{
    public ValidateOptionsResult Validate(string? name, EndpointOptions options) =>
        ValidateOptionsResult.Fail("Use ValidateOnStart and await startup before synchronous options access.");
    public async Task<ValidateOptionsResult> ValidateAsync(string? name, EndpointOptions options, CancellationToken token)
    {
        return await probe.CheckAsync(options, token) ? ValidateOptionsResult.Success :
            ValidateOptionsResult.Fail("Validation marker must contain READY before startup can complete.");
    }
}
