using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators
{
    public static class MemberAccessExpressionValidator
    {
        public static bool MethodNameShouldBe(this MemberAccessExpressionSyntax node, string checkArg)
        {
            return checkArg.Equals(node.Name.Identifier.ToString());
        }

        public static bool MethodNameShouldBeIn(this MemberAccessExpressionSyntax node, string[] checkArgs)
        {
            return checkArgs.Any(x =>
                x.Equals(node.Name.Identifier.ToString()));
        }

        public static bool LeftSideShouldBeIdentifier(this MemberAccessExpressionSyntax node, bool shouldBe = true)
        {
            return node.Expression
                is IdentifierNameSyntax == shouldBe;
        }
    }
}
