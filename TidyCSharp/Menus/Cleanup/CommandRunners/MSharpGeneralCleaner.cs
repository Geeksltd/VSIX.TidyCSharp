using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
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
                if (node.ToString().Length < 110)
                    return base.VisitExpressionStatement(node);

                var m = node.Expression;
                List<SyntaxToken> rewritableToken = new List<SyntaxToken>();
                var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                trivia = trivia.AddRange(node.GetLeadingTrivia().Reverse()
                    .TakeWhile(x => x.IsDirective ^ !x.IsKind(SyntaxKind.EndOfLineTrivia)));
                trivia = trivia.Add(SyntaxFactory.Tab);
                var newExpression = SyntaxFactory.ParseExpression("");
                while (m != null && m.ChildNodes().Any())
                {
                    var m2 = m.ChildNodes();
                    if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                        m2.LastOrDefault() is ArgumentListSyntax)
                    {
                        var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                        var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                        m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression;
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
                            newExpression = m.WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                               m.WithoutTrailingTrivia(), SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                        m = null;
                    }
                }
                if (m != null)
                    newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        m, SyntaxFactory.IdentifierName(newExpression.ToString()));
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