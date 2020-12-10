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
    public class MSharpGeneralCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return ChangeMethodHelper(initialSourceNode);
        }
        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new MultiLineExpressionRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CsMethodStringRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }
        class MultiLineExpressionRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public MultiLineExpressionRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                if (node.ToString().Split('\n').All(x => x.Length < 110))
                    return base.VisitExpressionStatement(node);

                var tokens = node.DescendantTokens().Where(x => x.IsKind(SyntaxKind.DotToken))
                    .Where(x => x.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                        x.Parent.Parent.IsKind(SyntaxKind.InvocationExpression));
                node = node.ReplaceTokens(tokens, (nde1, nde2) =>
                {
                    var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                    trivia = trivia.AddRange(node.GetLeadingTrivia()
                            .Where(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)));
                    trivia = trivia.Add(SyntaxFactory.Tab);
                    return nde2.WithLeadingTrivia(trivia);
                });
                return node;
            }
        }
        class CsMethodStringRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CsMethodStringRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (node.Token.ValueText.StartsWith("c#:"))
                {
                    var args = new SeparatedSyntaxList<ArgumentSyntax>();
                    args = args.Add(SyntaxFactory.Argument(
                        SyntaxFactory.ParseExpression("\"" + node.Token.Text.TrimStart("\"c#:"))));
                    return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("cs")
                        , SyntaxFactory.ArgumentList(args));
                }
                return base.VisitLiteralExpression(node);
            }
        }

    }
}
