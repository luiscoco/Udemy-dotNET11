using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
namespace Course.Generators;
[Generator(LanguageNames.CSharp)]
public sealed class EventGenerator:IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor Invalid=new("GEN252","Invalid event shape",
        "Event must be a top-level public nongeneric partial class with public string Id","Contract",DiagnosticSeverity.Error,true);
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var events=context.SyntaxProvider.ForAttributeWithMetadataName("Course.GenerateEventAttribute",
            (node,_)=>node is ClassDeclarationSyntax,(c,_)=>(INamedTypeSymbol)c.TargetSymbol).WithTrackingName("EventContracts");
        context.RegisterSourceOutput(events,(output,symbol)=>
        {
            bool partial=symbol.DeclaringSyntaxReferences.Select(r=>r.GetSyntax()).OfType<ClassDeclarationSyntax>()
                .Any(c=>c.Modifiers.Any(SyntaxKind.PartialKeyword));
            bool id=symbol.GetMembers("Id").OfType<IPropertySymbol>().Any(p=>p.DeclaredAccessibility==Accessibility.Public&&p.Type.SpecialType==SpecialType.System_String&&!p.IsStatic);
            if(!partial||!id||symbol.DeclaredAccessibility!=Accessibility.Public||symbol.ContainingType!=null||symbol.TypeParameters.Length!=0||symbol.GetMembers("Describe").Length>0)
            {output.ReportDiagnostic(Diagnostic.Create(Invalid,symbol.Locations.FirstOrDefault()));return;}
            string ns=symbol.ContainingNamespace.IsGlobalNamespace?"":"namespace "+symbol.ContainingNamespace.ToDisplayString()+";";
            string source=ns+$$"""public partial class @{{symbol.Name}} {public static string EventName => "{{symbol.Name}}"; public string Describe()=>EventName+":"+Id;}""";
            output.AddSource(symbol.Name+".Event.g.cs",SourceText.From(source,Encoding.UTF8));
        });
    }
}
