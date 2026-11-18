using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed record Finding(string Rule, int Line, string Severity, string Detail);
public static class Review
{
    public static List<Finding> Analyze(string text)
    {
        var syntax = CSharpSyntaxTree.ParseText(text);
        var findings = new List<Finding>();
        foreach (var assignment in syntax.GetRoot().DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left.ToString().EndsWith("CommandText") && assignment.Right.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Any(e => e.IsKind(SyntaxKind.AddExpression)))
                findings.Add(new("SQL001", assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1, "High", "SQL concatenates values; use parameters and preserve the tenant predicate."));
        }

        return findings;
    }
}
