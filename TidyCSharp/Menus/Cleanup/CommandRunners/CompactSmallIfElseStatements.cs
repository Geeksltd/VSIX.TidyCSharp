using System;
using System.Linq;
using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CompactSmallIfElseStatements : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return CompactSmallIfElseStatementsHelper(initialSourceNode);
        }

        public static SyntaxNode CompactSmallIfElseStatementsHelper(SyntaxNode initialSourceNode)
        {
            var newRoot = new Rewriter(initialSourceNode).Visit(initialSourceNode);

            if (newRoot != initialSourceNode && TidyCSharpPackage.Instance != null)
            {
                initialSourceNode = Formatter.Format(newRoot, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
            }
            return initialSourceNode;
        }

        static SyntaxTrivia _endOfLineTrivia = default(SyntaxTrivia);
        const int MAX_IF_LINE_LENGTH = 50;
        const int MAX_RETURN_STATEMENT_LENGTH = 10;

        class Rewriter : CSharpSyntaxRewriter
        {
            public Rewriter(SyntaxNode initialSource) : base()
            {
                _endOfLineTrivia =
                    initialSource
                        .SyntaxTree
                        .GetRoot()
                        .DescendantTrivia(descendIntoTrivia: true)
                        .FirstOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia));
            }

            public override SyntaxNode VisitIfStatement(IfStatementSyntax mainIFnode)
            {
                var newIfNode = Cleanup(mainIFnode);

                if (!string.Equals(newIfNode.ToFullString().Trim(), mainIFnode.ToFullString().Trim(), StringComparison.Ordinal))
                {
                    var nextToken = mainIFnode.GetLastToken().GetNextToken();

                    if
                    (
                         nextToken != null &&
                         mainIFnode.GetTrailingTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia)) == false &&
                         nextToken.LeadingTrivia.Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia)) == false
                    )
                    {
                        return newIfNode.WithTrailingTrivia(newIfNode.GetTrailingTrivia().Add(_endOfLineTrivia));
                    }
                    return newIfNode.WithTrailingTrivia(newIfNode.GetTrailingTrivia());
                }
                return mainIFnode;
            }

            SyntaxNode Cleanup(IfStatementSyntax originalIfNode)
            {
                if (CanCleanupIF(originalIfNode) == false) return base.VisitIfStatement(originalIfNode);

                var singleStatementInsideIf = GetInsideStatement(originalIfNode.Statement);
                if (singleStatementInsideIf != null && singleStatementInsideIf is IfStatementSyntax == false)
                {
                    originalIfNode = GetNewIF(originalIfNode, singleStatementInsideIf);
                }

                if (originalIfNode.Else != null)
                {
                    var singleStatementInsideElse = GetInsideStatement(originalIfNode.Else.Statement);
                    if (singleStatementInsideElse != null)
                    {
                        if (singleStatementInsideElse is IfStatementSyntax ifSingleStatement)
                        {
                            singleStatementInsideElse = Cleanup(ifSingleStatement) as IfStatementSyntax;
                        }

                        originalIfNode = GetNewIfWithElse(originalIfNode, singleStatementInsideElse);
                    }
                }

                return originalIfNode;
            }

            private bool CanCleanupIF(IfStatementSyntax originalIfNode)
            {
                if (originalIfNode.DescendantTrivia(descendIntoTrivia: true).Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia)) == false) return false;
                return true;
            }

            IfStatementSyntax GetNewIF(IfStatementSyntax orginalIFnode, StatementSyntax singleStatementInsideIf)
            {
                var newIf =
                    orginalIFnode
                        .WithIfKeyword(orginalIFnode.IfKeyword.WithTrailingTrivia(SyntaxFactory.Space))
                        .WithOpenParenToken(orginalIFnode.OpenParenToken.WithoutWhitespaceTrivia())
                        .WithCloseParenToken(orginalIFnode.CloseParenToken.WithoutWhitespaceTrivia())
                        .WithCondition(orginalIFnode.Condition.WithoutWhitespaceTrivia())
                        .WithStatement(
                            singleStatementInsideIf
                                .WithLeadingTrivia(SyntaxFactory.Space)
                        );

                if (singleStatementInsideIf is ReturnStatementSyntax returnStatement)
                {
                    if
                    (
                        returnStatement.Expression == null ||
                        (
                            newIf.WithElse(null).Span.Length <= MAX_IF_LINE_LENGTH
                            &&
                            (returnStatement.Expression is LiteralExpressionSyntax || returnStatement.Expression.Span.Length <= MAX_RETURN_STATEMENT_LENGTH)
                        )
                    )
                        return newIf;

                    if (singleStatementInsideIf.WithoutTrivia().DescendantTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia))) return orginalIFnode;
                    if (newIf.WithElse(null).WithoutTrivia().Span.Length <= MAX_IF_LINE_LENGTH) return newIf;
                    return orginalIFnode;
                }
                if (singleStatementInsideIf.WithoutTrivia().DescendantTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia))) return orginalIFnode;
                if (newIf.WithElse(null).Span.Length > MAX_IF_LINE_LENGTH) return orginalIFnode;

                return newIf;
            }

            IfStatementSyntax GetNewIfWithElse(IfStatementSyntax ifNode, StatementSyntax singleStatementInsideElse)
            {
                var newElse =
                        ifNode.Else
                            .WithElseKeyword(
                                ifNode.Else.ElseKeyword
                                .WithTrailingTrivia(
                                    ifNode.Else.ElseKeyword.LeadingTrivia.WithoutWhitespaceTrivia()
                                )
                            )
                            .WithStatement(singleStatementInsideElse.WithLeadingTrivia(SyntaxFactory.Space));

                if (singleStatementInsideElse is ReturnStatementSyntax returnStatement)
                {
                    if (returnStatement.Expression == null || returnStatement.Expression is LiteralExpressionSyntax || returnStatement.Expression.Span.Length <= MAX_RETURN_STATEMENT_LENGTH)
                        return ifNode.WithElse(newElse);
                }

                if (singleStatementInsideElse is IfStatementSyntax == false)
                {
                    if (
                        singleStatementInsideElse.WithoutTrivia().DescendantTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia))
                        ||
                        singleStatementInsideElse.Span.Length + 5 > MAX_IF_LINE_LENGTH
                       )
                        return ifNode.WithElse(ifNode.Else.WithStatement(singleStatementInsideElse));
                }



                return ifNode.WithElse(newElse);
            }

            StatementSyntax GetInsideStatement(BlockSyntax block)
            {
                if (block.Statements.Count != 1) return null;
                if (block.HasNoneWhitespaceTrivia()) return null;
                var firstStatement = block.Statements.First();
                if (firstStatement is IfStatementSyntax) return firstStatement;
                if (firstStatement.Span.Length <= NormalizeWhitespace.Options.BLOCK_SINGLE_STATEMENT_MAX_LENGTH) return firstStatement;
                return null;
            }

            StatementSyntax GetInsideStatement(StatementSyntax singleStatement)
            {
                if (singleStatement is BlockSyntax newBlockStatement)
                {
                    if ((singleStatement = GetInsideStatement(newBlockStatement)) == null) return null;
                }

                if (singleStatement.HasNoneWhitespaceTrivia()) return null;

                return singleStatement;
            }
        }
    }
}