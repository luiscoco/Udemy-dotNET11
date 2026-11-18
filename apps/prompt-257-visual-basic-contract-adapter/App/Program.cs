using Legacy.Contracts;
decimal total=LegacyAdapter.Total(12.50m,0.20m,false);
decimal midpoint=LegacyAdapter.Total(10.005m,0m,args.Contains("problem"));
Console.WriteLine($"VB total={total:F2} midpoint={midpoint:F2} expectedMidpoint=10.01");
Check.That(total==15m&&midpoint==10.01m,"the CSharp integration preserves the VB decimal and midpoint-rounding contract","EXT legacy rounding contract changed");
bool invalid=false;try{TaxCalculator.Total(10m,2m);}catch(ArgumentOutOfRangeException){invalid=true;}
Check.That(invalid,"invalid input is rejected by the actual VB library");
var method=typeof(TaxCalculator).GetMethod("Total")!;
Check.That(method.ReturnType==typeof(decimal)&&method.GetParameters().All(p=>p.ParameterType==typeof(decimal)),"the public library contract remains strongly typed Decimal");
Harness.Save("ext-evidence.json",new {passed=true,actualVisualBasic=true,total,midpoint,invalid,libraryFramework=typeof(TaxCalculator).Assembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute),false)
    .Cast<System.Runtime.Versioning.TargetFrameworkAttribute>().Single().FrameworkName});
