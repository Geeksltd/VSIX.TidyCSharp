using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class SimplyAsyncCalls : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return SimplyAsyncCallsHelper(initialSourceNode, Options);
            // return SimplyAsyncCallsHelper2(initialSourceNode);
        }

        public static SyntaxNode SimplyAsyncCallsHelper2(SyntaxNode initialSourceNode, ICleanupOption options)
        {
            return new Rewriter(options).Visit(initialSourceNode);
        }
        class Rewriter : CleanupCSharpSyntaxRewriter
        {
            public Rewriter(ICleanupOption options) : base(options)
            {
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null) return base.Visit(node);
                if (node is MethodDeclarationSyntax == false) return base.Visit(node);
                if (node.Parent is ClassDeclarationSyntax == false) return base.Visit(node);

                node = SimplyAsyncCallsHelper((MethodDeclarationSyntax)node, Options);

                return base.Visit(node);
            }
        }

        static SyntaxTrivia[] _spaceTrivia = { SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ") };

        public static SyntaxNode SimplyAsyncCallsHelper(SyntaxNode initialSource, ICleanupOption options)
        {
            return
                initialSource
                    .ReplaceNodes
                    (
                        initialSource
                            .DescendantNodes()
                            .Where(node => node is MethodDeclarationSyntax && node.Parent is ClassDeclarationSyntax)
                        , (node1, node2) => SimplyAsyncCallsHelper((MethodDeclarationSyntax)node1, options)
                    );
        }

        public static MethodDeclarationSyntax SimplyAsyncCallsHelper(MethodDeclarationSyntax method, ICleanupOption options)
        {
            if ((method.Parent is ClassDeclarationSyntax) == false) return method;
            if (method.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword)) == false) return method;
            if (method.Body == null) return method;
            if (method.ReturnType.WithoutTrivia().ToFullString() == typeof(Task).Name) return method;
            if (method.ReturnType.WithoutTrivia().ToFullString() == "void") return method;
            if (method.Body.Statements.Count != 1) return method;

            var singleStatement = method.Body.Statements.First();

            AwaitExpressionSyntax awaitStatementExpression = null;

            if (singleStatement is ReturnStatementSyntax retSS)
            {
                awaitStatementExpression = retSS.Expression as AwaitExpressionSyntax;
            }
            else if (singleStatement is ExpressionStatementSyntax eSS)
            {
                awaitStatementExpression = eSS.Expression as AwaitExpressionSyntax;
            }

            if (awaitStatementExpression == null) return method;

            if (awaitStatementExpression.Expression is InvocationExpressionSyntax invSS)
            {
                if (invSS.ArgumentList.Arguments.Any(a => a.Expression.IsKind(SyntaxKind.AwaitExpression)))
                    return method;
            }

            var newStatement = singleStatement;

            if (options.Should((int)SimplyAsyncCall.CleanupTypes.Single_Expression))
            {
                if (singleStatement is ReturnStatementSyntax rss)
                {
                    newStatement = rss.WithExpression(awaitStatementExpression.Expression);
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