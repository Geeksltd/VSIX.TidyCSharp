using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeTypeConverter;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeValidators;

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