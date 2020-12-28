using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
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
                return ChangeMethodHelper(initialSourceNode);
            return initialSourceNode;
        }
        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new LocalTimeRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CascadeDeleteRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
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
        class CascadeDeleteRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CascadeDeleteRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnDelete").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.ArgumentsCountShouldBe(1) &&
                    s.FirstArgumentShouldBe("CascadeAction.CascadeDelete"))
                {
                    var newNode = node.ReplaceNode(s.ArgumentList, SyntaxFactory.ArgumentList());

                    newNode = newNode.ReplaceNode(newNode.DescendantNodesAndSelfOfType<IdentifierNameSyntax>()
                            .FirstOrDefault(x => x.Identifier.ToString() == "OnDelete"),
                                SyntaxFactory.IdentifierName("CascadeDelete"));
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
        }
    }
}
