using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeTypeConverter;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public static class ArgumentListSyntaxExtractor
{
    public static BlockSyntax GetBlockSyntaxOfFirstArgument(this ArgumentListSyntax node)
    {
        return node.Arguments.FirstOrDefault()
            .Expression.As<SimpleLambdaExpressionSyntax>()
            ?.Body.As<BlockSyntax>();
    }

    public static ArgumentSyntax FirstArgument(this ArgumentListSyntax node)
    {
        return node.Arguments.FirstOrDefault();
    }
}