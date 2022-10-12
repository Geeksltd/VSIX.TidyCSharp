using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public interface ISyntaxTreeMemberExtractor
{
    List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind);
}