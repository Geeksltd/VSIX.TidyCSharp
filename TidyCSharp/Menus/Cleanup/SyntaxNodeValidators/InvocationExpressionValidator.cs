using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
