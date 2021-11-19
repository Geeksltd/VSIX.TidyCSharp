using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators
{
    public static class ExpressionStatementValidator
    {
        public static bool MethodNameShouldBe(this ExpressionStatementSyntax node, string checkArg)
        {
            return checkArg.Equals(node.Expression.As<MemberAccessExpressionSyntax>()?.Name.ToString());
        }

        public static bool IdentifierShouldBe(this ExpressionStatementSyntax node, string checkArg)
        {
            return checkArg.Equals(node.FirstDescendantNode<MemberAccessExpressionSyntax>().Expression.ToString());
        }
    }
}
