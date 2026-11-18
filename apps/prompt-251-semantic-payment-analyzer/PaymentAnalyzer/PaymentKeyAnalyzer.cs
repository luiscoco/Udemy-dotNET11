using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
namespace Course.Analyzers;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PaymentKeyAnalyzer:DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule=new("PAY251","Payment needs an idempotency key",
        "Payment operations require a nonempty idempotency key","Reliability",DiagnosticSeverity.Error,true);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics=>[Rule];
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);context.EnableConcurrentExecution();
        context.RegisterOperationAction(c=>
        {
            var call=(IInvocationOperation)c.Operation;
            if(call.TargetMethod.Name!="Charge"||call.TargetMethod.ContainingType.ToDisplayString()!="Course.Payments.Gateway")return;
            foreach(var argument in call.Arguments)
                if(argument.Parameter?.Name=="idempotencyKey"&&argument.Value.ConstantValue.HasValue
                    &&string.IsNullOrWhiteSpace(argument.Value.ConstantValue.Value as string))
                    c.ReportDiagnostic(Diagnostic.Create(Rule,argument.Syntax.GetLocation()));
        },OperationKind.Invocation);
    }
}
