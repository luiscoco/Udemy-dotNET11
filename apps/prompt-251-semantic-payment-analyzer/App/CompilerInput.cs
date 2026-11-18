using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
public static class CompilerInput
{
    public static CSharpCompilation Create(string source)=>CSharpCompilation.Create("Fixture_"+Guid.NewGuid().ToString("N"),
        [CSharpSyntaxTree.ParseText(source)],((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p=>MetadataReference.CreateFromFile(p)),new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
