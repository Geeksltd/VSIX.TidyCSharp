using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpModelCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            if (App.DTE.ActiveDocument.ProjectItem.ProjectItems.ContainingProject.Name == "#Model")
                return ChangeMethodHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
            return initialSourceNode;
        }
        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            initialSourceNode = new Rewriter(semanticModel).Visit(initialSourceNode);

            return initialSourceNode;
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public Rewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                var methodType = methodSymbol?.ReturnType.OriginalDefinition?.ToString();
                var acceptedArguments = new string[]
                {
                    "\"c#:LocalTime.UtcNow\"","\"c#:LocalTime.Now\"",
                    "cs(\"LocalTime.UtcNow\")","cs(\"LocalTime.Now\")"
                };
                if (methodName == "Default" && methodType == "MSharp.DateTimeProperty")
                {
                    if (node.ArgumentList.Arguments.Count == 1 &&
                        acceptedArguments.Any(x => x.Equals(node.ArgumentList.Arguments.FirstOrDefault().ToString())))
                    {
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, node.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault(), SyntaxFactory.IdentifierName("DefaultToNow")),
                            SyntaxFactory.ArgumentList());
                    }
                }

                return base.VisitInvocationExpression(node);
            }
        }
    }
}
