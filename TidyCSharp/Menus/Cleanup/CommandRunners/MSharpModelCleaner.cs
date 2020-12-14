using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators;
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
            initialSourceNode = new LocalTimeRewriter(semanticModel).Visit(initialSourceNode);

            return initialSourceNode;
        }

        class LocalTimeRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public LocalTimeRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

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
                    if (node.ArgumentsCountShouldBe(1) &&
                        node.FirstArgumentShouldBeIn(acceptedArguments))
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
