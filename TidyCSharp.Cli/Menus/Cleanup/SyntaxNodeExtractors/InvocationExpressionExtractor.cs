using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeTypeConverter;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeValidators;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public static class InvocationExpressionExtractor
{
    public static string GetLeftSideIdentifier(this InvocationExpressionSyntax node)
    {
        return node.Expression.As<MemberAccessExpressionSyntax>()?.Expression.ToString();
    }

    public static ExpressionSyntax GetLeftSideExpression(this InvocationExpressionSyntax node)
    {
        return node.Expression.As<MemberAccessExpressionSyntax>()?.Expression;
    }

    public static SimpleNameSyntax GetRightSideNameSyntax(this InvocationExpressionSyntax node)
    {
        return node.Expression.As<MemberAccessExpressionSyntax>()?.Name;
    }

    public static ArgumentListSyntax GetArgumentsOfMethod(this InvocationExpressionSyntax node, string methodName)
    {
        // var d1 = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>();
        // var d2 = d1.Where(x => x.MethodNameShouldBe(methodName));
        // var d3 = d2.FirstOrDefault().ArgumentList;
        return node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
            .Where(x => x.MethodNameShouldBe(methodName))?.FirstOrDefault()?.ArgumentList;
    }

    public static ArgumentSyntax FirstArgument(this InvocationExpressionSyntax node)
    {
        return node.ArgumentList.Arguments.FirstOrDefault();
    }
}