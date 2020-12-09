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
            return ChangeMethodHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
        }
        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            initialSourceNode = new MultiLineExpressionRewriter(semanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }
        class MultiLineExpressionRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            int lastNewLinePosition;
            public MultiLineExpressionRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var tokens = node.DescendantTokens().Where(x => x.IsKind(SyntaxKind.DotToken))
                    .Where(x => x.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                        x.Parent.Parent.IsKind(SyntaxKind.InvocationExpression));
                node = node.ReplaceTokens(tokens, (nde1, nde2) =>
                {
                    if (lastNewLinePosition == 0)
                    {
                        lastNewLinePosition = ((MemberAccessExpressionSyntax)nde2.Parent).Expression.SpanStart;
                    }
                    var lastEndOfLineTrivia = ((MemberAccessExpressionSyntax)nde2.Parent).DescendantTrivia()
                    .Where(x => x.IsKind(SyntaxKind.EndOfLineTrivia)).Select(x => x.SpanStart);
                    if (lastEndOfLineTrivia.Any())
                    {
                        if (lastEndOfLineTrivia.LastOrDefault() > lastNewLinePosition)
                            lastNewLinePosition = lastEndOfLineTrivia.LastOrDefault();
                    }
                    if (((MemberAccessExpressionSyntax)nde2.Parent).Expression.Span.End - lastNewLinePosition > 50)
                    {
                        var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                        trivia = trivia.AddRange(nde2.Parent.GetLeadingTrivia()
                            .Where(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)));
                        lastNewLinePosition = nde1.SpanStart;
                        return nde2.WithLeadingTrivia(trivia);
                    }
                    return nde2;
                });
                return node;
            }
        }
    }
}
