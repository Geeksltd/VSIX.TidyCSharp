using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Geeks.GeeksProductivityTools.Menus.Cleanup;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class SimplyAsyncCalls : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return SimplyAsyncCallsHelper(initialSourceNode);
            //return SimplyAsyncCallsHelper2(initialSourceNode);
        }


        public static SyntaxNode SimplyAsyncCallsHelper2(SyntaxNode initialSourceNode)
        {
            return new Rewriter().Visit(initialSourceNode);
        }
        class Rewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null) return base.Visit(node);
                if (node is MethodDeclarationSyntax == false) return base.Visit(node);
                if (node.Parent is ClassDeclarationSyntax == false) return base.Visit(node);

                node = SimplyAsyncCallsHelper((MethodDeclarationSyntax)node);

                return base.Visit(node);
            }
        }

        static SyntaxTrivia[] _spaceTrivia = { SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ") };

        public static SyntaxNode SimplyAsyncCallsHelper(SyntaxNode initialSource)
        {
            return
                initialSource
                    .ReplaceNodes
                    (
                        initialSource
                            .DescendantNodes()
                            .Where(node => node is MethodDeclarationSyntax && node.Parent is ClassDeclarationSyntax)
                        , (node1, node2) => SimplyAsyncCallsHelper((MethodDeclarationSyntax)node1)
                    );
        }

        public static MethodDeclarationSyntax SimplyAsyncCallsHelper(MethodDeclarationSyntax method)
        {
            if ((method.Parent is ClassDeclarationSyntax) == false) return method;
            if (method.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword)) == false) return method;
            if (method.Body == null) return method;
            if (method.ReturnType.WithoutTrivia().ToFullString() == typeof(Task).Name) return method;
            if (method.ReturnType.WithoutTrivia().ToFullString() == "void") return method;
            if (method.Body.Statements.Count != 1) return method;

            var singleStatement = method.Body.Statements.First();

            AwaitExpressionSyntax awaitStatementExpression = null;

            if (singleStatement is ReturnStatementSyntax)
            {
                awaitStatementExpression =
                    (singleStatement as ReturnStatementSyntax).Expression as AwaitExpressionSyntax;
            }
            else if (singleStatement is ExpressionStatementSyntax)
            {
                awaitStatementExpression =
                    (singleStatement as ExpressionStatementSyntax).Expression as AwaitExpressionSyntax;
            }

            if (awaitStatementExpression == null) return method;

            var newStatement = singleStatement;

            if (singleStatement is ReturnStatementSyntax)
            {
                newStatement =
                    (singleStatement as ReturnStatementSyntax).WithExpression(awaitStatementExpression.Expression);
            }
            else if (singleStatement is ExpressionStatementSyntax)
            {
                var newReturnStatement =
                    SyntaxFactory
                        .ReturnStatement(awaitStatementExpression.Expression)
                        .WithLeadingTrivia(singleStatement.GetLeadingTrivia())
                        .WithTrailingTrivia(singleStatement.GetTrailingTrivia());

                newStatement =
                    newReturnStatement.WithReturnKeyword(
                        newReturnStatement.ReturnKeyword.WithTrailingTrivia(_spaceTrivia));
            }

            return
                method
                    .ReplaceNode(singleStatement, newStatement)
                    .WithModifiers(
                        method.Modifiers.Remove(method.Modifiers.First(x => x.IsKind(SyntaxKind.AsyncKeyword))))
                    .WithLeadingTrivia(method.GetLeadingTrivia())
                    .WithTrailingTrivia(method.GetTrailingTrivia());
        }
    }
}