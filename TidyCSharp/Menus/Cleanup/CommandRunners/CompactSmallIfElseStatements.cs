using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CompactSmallIfElseStatements : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            var syntaxRewriter = new Rewriter(initialSourceNode, IsReportOnlyMode, Options);
            var modifiedSourceNode = syntaxRewriter.Visit(initialSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(syntaxRewriter.GetReport());
                return initialSourceNode;
            }

            if (modifiedSourceNode != initialSourceNode && TidyCSharpPackage.Instance != null)
            {
                initialSourceNode = Formatter.Format(modifiedSourceNode, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            return initialSourceNode;
        }

        static SyntaxTrivia _endOfLineTrivia = default(SyntaxTrivia);
        const int MAX_IF_LINE_LENGTH = 50, MAX_RETURN_STATEMENT_LENGTH = 10;

        class Rewriter : CleanupCSharpSyntaxRewriter
        {
            public Rewriter(SyntaxNode initialSource, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options)
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

                    var lineSpan = mainIFnode.GetFileLinePosSpan();

                    AddReport(new ChangesReport(mainIFnode)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Compact small if-else statements",
                        Generator = nameof(CompactSmallIfElseStatements)
                    });

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
                if (originalIfNode.DescendantTrivia(descendIntoTrivia: true).HasNoneWhiteSpaceTrivia()) return base.VisitIfStatement(originalIfNode);

                var singleStatementInsideIf = GetInsideStatement(originalIfNode.Statement);

                if (singleStatementInsideIf == null || singleStatementInsideIf is IfStatementSyntax) return base.VisitIfStatement(originalIfNode);

                var newIf = GetNewIF(originalIfNode, singleStatementInsideIf);

                if (newIf == originalIfNode) return base.VisitIfStatement(originalIfNode);
                if (newIf.Else == null) return newIf;

                var singleStatementInsideElse = GetInsideStatement(newIf.Else.Statement);

                if (singleStatementInsideElse == null) return base.VisitIfStatement(originalIfNode);

                if (singleStatementInsideElse is IfStatementSyntax ifSingleStatement)
                {
                    singleStatementInsideElse = Cleanup(ifSingleStatement) as IfStatementSyntax;
                }

                newIf = newIf.WithElse(GetNewIfWithElse(newIf.Else, singleStatementInsideElse));

                return newIf;
            }

            IfStatementSyntax GetNewIF(IfStatementSyntax orginalIFnode, StatementSyntax singleStatementInsideIf)
            {
                var closeParenTrivia = orginalIFnode.CloseParenToken.WithoutWhiteSpaceTrivia();

                var trailingTriviaList =
                    new SyntaxTriviaList()
                    .AddRange(closeParenTrivia.TrailingTrivia.Where(t => t.IsWhiteSpaceTrivia() == false))
                    .AddRange(singleStatementInsideIf.GetTrailingTrivia().Where(t => t.IsWhiteSpaceTrivia() == false));

                if (singleStatementInsideIf != orginalIFnode.Statement)
                {
                    trailingTriviaList = trailingTriviaList.AddRange(orginalIFnode.Statement.GetTrailingTrivia().Where(t => t.IsWhiteSpaceTrivia() == false));
                }

                trailingTriviaList = trailingTriviaList.Add(_endOfLineTrivia);

                var newIf =
                    orginalIFnode
                        .WithIfKeyword(orginalIFnode.IfKeyword.WithTrailingTrivia(SyntaxFactory.Space))
                        .WithOpenParenToken(orginalIFnode.OpenParenToken.WithoutWhiteSpaceTrivia())
                        .WithCloseParenToken(SyntaxTokenExtensions.WithoutTrivia(closeParenTrivia))
                        .WithCondition(orginalIFnode.Condition.WithoutWhiteSpaceTrivia())
                        .WithStatement(
                            singleStatementInsideIf
                                .WithLeadingTrivia(SyntaxFactory.Space)
                                .WithTrailingTrivia(trailingTriviaList)
                        );

                if (singleStatementInsideIf is ReturnStatementSyntax returnStatement)
                {
                    if
                    (
                        (returnStatement.Expression == null && newIf.WithElse(null).Span.Length <= 2 * MAX_IF_LINE_LENGTH) ||
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

            ElseClauseSyntax GetNewIfWithElse(ElseClauseSyntax elseNode, StatementSyntax singleStatementInsideElse)
            {
                var newElse =
                        elseNode
                            .WithElseKeyword(
                                elseNode.ElseKeyword
                                .WithTrailingTrivia()
                            )
                            .WithStatement(
                                singleStatementInsideElse
                                .WithLeadingTrivia(SyntaxFactory.Space)
                            //.WithTrailingTrivia(trailingTriviaList)
                            );

                if (singleStatementInsideElse is ReturnStatementSyntax returnStatement)
                {
                    if (returnStatement.Expression == null || returnStatement.Expression is LiteralExpressionSyntax || returnStatement.Expression.Span.Length <= MAX_RETURN_STATEMENT_LENGTH)
                        return newElse;
                }

                if (singleStatementInsideElse is IfStatementSyntax == false)
                {
                    if (
                        singleStatementInsideElse.WithoutTrivia().DescendantTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia))
                        ||
                        singleStatementInsideElse.Span.Length + 5 > MAX_IF_LINE_LENGTH
                       )
                        return elseNode.WithStatement(singleStatementInsideElse);
                }

                return newElse;
            }

            StatementSyntax GetInsideStatement(BlockSyntax block)
            {
                if (block.Statements.Count != 1) return null;
                if (block.HasNoneWhiteSpaceTrivia()) return null;
                var firstStatement = block.Statements.FirstOrDefault();
                if (firstStatement is IfStatementSyntax) return firstStatement;
                if (firstStatement.Span.Length <= NormalizeWhiteSpace.Options.Block_Single_Statement_Max_Length) return firstStatement;
                return null;
            }

            StatementSyntax GetInsideStatement(StatementSyntax singleStatement)
            {
                if (singleStatement is BlockSyntax newBlockStatement)
                {
                    if ((singleStatement = GetInsideStatement(newBlockStatement)) == null) return null;
                }

                if (singleStatement.HasNoneWhiteSpaceTrivia()) return null;

                return singleStatement;
            }
        }
    }
}