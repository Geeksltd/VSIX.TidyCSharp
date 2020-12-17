using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators
{
    public static class MemberAccessExpressionValidator
    {
        public static bool MethodNameShouldBe(this MemberAccessExpressionSyntax node, string checkArg)
        {
            return checkArg.Equals(node.Name.ToString());
        }

        public static bool MethodNameShouldBeIn(this MemberAccessExpressionSyntax node, string[] checkArgs)
        {
            return checkArgs.Any(x =>
                x.Equals(node.Name.ToString()));
        }

        public static bool LeftSideShouldBeIdentifier(this MemberAccessExpressionSyntax node, bool shouldBe = true)
        {
            return node.Expression
                is IdentifierNameSyntax == shouldBe;
        }
    }
}
