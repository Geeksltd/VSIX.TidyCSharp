using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
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
}
