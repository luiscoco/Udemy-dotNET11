using Microsoft.AspNetCore.Components.Server.Circuits;
public sealed class TrackedCircuit : CircuitHandler
{
    public string Id { get; private set; } = "";
    private Circuit? Current;
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Id = circuit.Id;
        Current = circuit;
        return Task.CompletedTask;
    }
    public Task<bool> RequestPauseAsync(CancellationToken cancellationToken = default) =>
        Current?.RequestCircuitPauseAsync(cancellationToken) ?? Task.FromResult(false);
}
