using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators
{
    public static class InvocationExpressionValidator
    {
        public static bool ArgumentsCountShouldBe(this InvocationExpressionSyntax node, int count)
        {
            return node.ArgumentList.Arguments.Count() == count;
        }

        public static bool FirstArgumentShouldBeIn(this InvocationExpressionSyntax node, string[] checkArgs)
        {
            return checkArgs.Any(x =>
                x.Equals(node.ArgumentList.Arguments.FirstOrDefault().ToString()));
        }

        public static bool FirstArgumentShouldBe(this InvocationExpressionSyntax node, string checkArg)
        {
            return checkArg.Equals(node.ArgumentList.Arguments.FirstOrDefault().ToString());
        }

        public static bool LastArgumentShouldBe(this InvocationExpressionSyntax node, string checkArg)
        {
            return checkArg.Equals(node.ArgumentList.Arguments.LastOrDefault().ToString());
        }

        public static bool MethodNameShouldBeIn(this InvocationExpressionSyntax node, string[] checkArgs)
        {
            return checkArgs.Any(x =>
                x.Equals(node.Expression.As<MemberAccessExpressionSyntax>()?.Name.Identifier.ToString()));
        }

        public static bool MethodNameShouldBe(this InvocationExpressionSyntax node, string checkArg)
        {
            return checkArg.Equals(node.Expression.As<MemberAccessExpressionSyntax>()?.Name.Identifier.Text);
        }

        public static bool LeftSideShouldBeIdentifier(this InvocationExpressionSyntax node, bool shouldBe = true)
        {
            return node.Expression.As<MemberAccessExpressionSyntax>()?.Expression
                is IdentifierNameSyntax == shouldBe;
        }
    }
}