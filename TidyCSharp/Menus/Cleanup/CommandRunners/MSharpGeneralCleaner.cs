using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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
            initialSourceNode = new CsMethodStringRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new MultiLineExpressionRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }
        class MultiLineExpressionRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public MultiLineExpressionRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                // if (node.ToString().Split('\n').All(x => x.Length < 110))
                //    return base.VisitExpressionStatement(node);

                // var tokens = node.DescendantTokens().Where(x => x.IsKind(SyntaxKind.DotToken))
                //    .Where(x => x.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                //        x.Parent.Parent.IsKind(SyntaxKind.InvocationExpression) &&
                //        x.Parent.Ancestors().All(x => !x.IsKind(SyntaxKind.ArgumentList)));
                // node = node.ReplaceTokens(tokens, (nde1, nde2) =>
                // {
                //    if (nde1.Parent.GetLeadingTrivia()
                //        .Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                //        return nde1;

                //    var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                //    trivia = trivia.AddRange(node.GetLeadingTrivia().Reverse()
                //        .TakeWhile(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)));
                //    trivia = trivia.Add(SyntaxFactory.Tab);
                //    return nde2.WithLeadingTrivia(trivia);
                // });
                // return node;

                if (node.ToString().Length < 110)
                    return base.VisitExpressionStatement(node);

                var m = node.Expression;
                List<SyntaxToken> rewritableToken = new List<SyntaxToken>();
                var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                trivia = trivia.AddRange(node.GetLeadingTrivia().Reverse()
                    .TakeWhile(x => !x.IsKind(SyntaxKind.EndOfLineTrivia)));
                trivia = trivia.Add(SyntaxFactory.Tab);
                var newExpression = SyntaxFactory.ParseExpression("");
                while (m != null && m.ChildNodes().Any())
                {
                    var m2 = m.ChildNodes();
                    if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                        m2.LastOrDefault() is ArgumentListSyntax)
                    {
                        var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                        var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                        m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression;
                        if (newExpression.ToString() == "")
                            newExpression = SyntaxFactory.InvocationExpression(methodName.Name, arguments)
                                .WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.InvocationExpression(methodName.Name, arguments).WithoutTrailingTrivia(),
                                SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                    }
                    else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                       m2.LastOrDefault() is ArgumentListSyntax)
                    {
                        var identifierName = m2.FirstOrDefault() as IdentifierNameSyntax;
                        var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                        m = null;
                        if (newExpression.ToString() == "")
                            newExpression = SyntaxFactory.InvocationExpression(identifierName, arguments).WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.InvocationExpression(identifierName.WithoutTrailingTrivia(), arguments.WithoutTrailingTrivia()),
                                    SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                    }
                    else
                    {
                        if (newExpression.ToString() == "")
                            newExpression = SyntaxFactory.InvocationExpression(m).WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.InvocationExpression(m),
                                    SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                        m = null;// (m2.FirstOrDefault() as ExpressionSyntax)?.Expression;
                    }
                }

                return SyntaxFactory.ExpressionStatement(newExpression)
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(node.GetTrailingTrivia());
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