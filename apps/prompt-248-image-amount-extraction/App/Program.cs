var amount=AmountExtractor.Extract("assets/amount-cells.png","EUR");
Console.WriteLine($"VISION total={amount.Total:F2} currency={amount.Currency} minimumCellConfidence={amount.CellConfidence.Min():F3} source={amount.SourcePage}");
Check.That(amount.Total==125.50m,"the actual vision model extracts the fixed-layout held-out digit amount");
Check.That(AmountExtractor.Validate(amount),"the extracted amount meets template, currency and confidence checks");
var invalid=amount with {Currency="XYZ"};
bool accepted=args.Contains("problem")||AmountExtractor.Validate(invalid);
Console.WriteLine($"VISION invalidCurrency={invalid.Currency} accepted={accepted}");
Check.That(!accepted,"structured output still requires independent semantic validation","VISION invalid currency passed validation");
bool badLayout=false;try{AmountExtractor.Extract("assets/wrong-layout.png","EUR");}catch(InvalidDataException){badLayout=true;}
Check.That(badLayout,"unsupported image geometry is rejected rather than inventing field locations");
Harness.Save("ml-evidence.json",new {passed=true,actualOnnxVision=true,amount,badLayout,invalidCurrencyRejected=!accepted,
    fixedTemplate=true,generalInvoiceOcr=false,confidenceCalibrated=false});
