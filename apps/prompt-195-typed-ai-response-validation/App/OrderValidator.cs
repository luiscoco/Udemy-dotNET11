using System.ComponentModel.DataAnnotations;
public sealed record OrderDraft(string Product, int Quantity);
public static class OrderValidator
{
    public static bool IsValid(OrderDraft draft) =>
        !string.IsNullOrWhiteSpace(draft.Product) && draft.Quantity is >= 1 and <= 10;
    public static OrderDraft Accept(OrderDraft draft, bool enforceBusinessRules)
    {
        if (enforceBusinessRules && !IsValid(draft))
            throw new ValidationException("Quantity must be between one and ten.");
        return draft;
    }
}
