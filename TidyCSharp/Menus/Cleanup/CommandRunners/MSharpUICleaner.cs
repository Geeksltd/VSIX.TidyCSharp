using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpUICleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            if (App.DTE.ActiveDocument.ProjectItem.ProjectItems.ContainingProject.Name == "#UI")
                return ChangeMethodHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
            return initialSourceNode;
        }
        private SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            initialSourceNode = new Rewriter(semanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }
        class Rewriter : CSharpSyntaxRewriter
        {
            private SemanticModel semanticModel;

            public Rewriter(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                var methodType = methodSymbol?.ReturnType.OriginalDefinition?.ToString();

                var methodDefTypes = new string[] {
                    "MSharp.PropertyFilterElement<TEntity>",
                    "MSharp.ViewElement<TEntity>",
                    "MSharp.ViewElement<TEntity, TProperty>",
                    "MSharp.NumberFormElement",
                    "MSharp.BooleanFormElement",
                    "MSharp.DateTimeFormElement",
                    "MSharp.StringFormElement",
                    "MSharp.AssociationFormElement",
                    "MSharp.BinaryFormElement",
                    "MSharp.CommonFilterElement<TEntity>"
                };
                var methodDefNames = new string[] { "Field", "Column", "Search" };

                if (methodDefTypes.Contains(methodType) && methodDefNames.Contains(methodName))
                {
                    if (node.ArgumentList.Arguments.Count == 1 && node.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax)
                    {
                        var arg = ((MemberAccessExpressionSyntax)((SimpleLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression).Body).Name;
                        return SyntaxFactory.ParseExpression($"{methodName.ToLower()}.{arg}()");
                    }
                }

                if (methodType == "MSharp.ListButton<TEntity>" || methodType == "MSharp.ModuleButton")
                {
                    switch (methodName)
                    {
                        case "ButtonColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Button"));
                        case "LinkColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Link"));
                        case "SearchButton":
                            return node.WithExpression(SyntaxFactory.ParseExpression("search.Button"));
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
    }
}