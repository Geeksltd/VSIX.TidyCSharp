using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public class PropertyExtractor : ISyntaxTreeMemberExtractor
{
    public List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind)
    {
        return root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Where(prp => prp.Modifiers.Any(m => m.IsKind(kind)))
            .SelectMany(method => method.Modifiers.Where(m => m.IsKind(kind)))
            .ToList();
    }
}