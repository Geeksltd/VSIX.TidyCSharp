using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public static class VariableDeclaratorSyntaxExtractor
{
    public static FileLinePositionSpan GetFileLinePosSpan(this VariableDeclaratorSyntax node)
    {
        return node.SyntaxTree.GetLineSpan(new TextSpan(node.Span.Start, node.Span.Length));
    }
}