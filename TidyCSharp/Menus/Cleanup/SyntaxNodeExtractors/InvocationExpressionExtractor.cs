using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
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
            return node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                 .Where(x => x.MethodNameShouldBe(methodName)).FirstOrDefault().ArgumentList;
        }

        public static ArgumentSyntax FirstArgument(this InvocationExpressionSyntax node)
        {
            return node.ArgumentList.Arguments.FirstOrDefault();
        }
    }
}
