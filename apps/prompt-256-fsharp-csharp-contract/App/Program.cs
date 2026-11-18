using Course.Functional;
var quote=PricingAdapter.Calculate(3,12.50m,false);var fractional=PricingAdapter.Calculate(3,0.10m,false);
Check.That(quote.Total==37.50m&&fractional.Total==0.30m,"the actual FSharp library returns precise decimal quotes through the CLR contract");
bool rejected=false;try{PricingAdapter.Calculate(0,12.50m,args.Contains("problem"));}catch(ArgumentException e){rejected=e.Message.Contains("Quantity");}
Console.WriteLine($"FSHARP total={quote.Total:F2} fractional={fractional.Total:F2} invalidQuantityRejected={rejected}");
Check.That(rejected,"FSharp Result errors remain failures at the CSharp boundary","EXT FSharp error became a successful default");
Check.That(Pricing.TryQuote(1,-1m).IsError,"the FSharp library independently rejects an invalid price");
Harness.Save("ext-evidence.json",new {passed=true,actualFSharp=true,total=quote.Total,fractional=fractional.Total,rejected,
    contractAssembly=typeof(Quote).Assembly.GetName().Name,coreAssembly=typeof(Microsoft.FSharp.Core.Unit).Assembly.GetName().Version?.ToString()});
