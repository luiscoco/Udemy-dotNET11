using System.ComponentModel;
using System.Security.Claims;
using Microsoft.Extensions.AI;
public sealed class InventoryTool(ClaimsPrincipal actor, bool enforceAuthorization)
{
    public int Executions { get; private set; }
    [Description("Reserve stock. Quantity must be between one and five.")]
    public string Reserve(int quantity)
    {
        if (enforceAuthorization && !actor.IsInRole("stock-writer"))
            throw new UnauthorizedAccessException("Stock writer role required.");
        if (quantity is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        Executions++;
        return $"Reserved {quantity} items.";
    }
    public AIFunction Export() => AIFunctionFactory.Create(Reserve);
}
