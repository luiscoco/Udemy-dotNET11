namespace Course.Functional

[<CLIMutable>]
type Quote = { Quantity: int; UnitPrice: decimal; Total: decimal }

module Pricing =
    [<CompiledName("TryQuote")>]
    let tryQuote (quantity: int, unitPrice: decimal) : Result<Quote, string> =
        if quantity < 1 || quantity > 10 then Error "Quantity must be between one and ten"
        elif unitPrice <= 0m then Error "Price must be positive"
        else Ok { Quantity = quantity; UnitPrice = unitPrice; Total = decimal quantity * unitPrice }
