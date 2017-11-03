using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Geeks.GeeksProductivityTools.Menus.Cleanup;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    public class WhitespaceRewriter : CSharpSyntaxRewriterBase
    {
        SyntaxToken _lastToken = default(SyntaxToken);
        MemberDeclarationSyntax _LastMember = null;
        bool _lastTokenIsAOpenBrace = false;
        bool _lastTokenIsACloseBrace = false;
        public WhitespaceRewriter(SyntaxNode initialSource, Options options) : base(initialSource, options) { }

        public SyntaxNode Apply()
        {
            TrimFile();

            return Visit(InitialSource);
        }

        void TrimFile()
        {
            var newLeadingTriviaList = CleanUpListWithNoWhitespaces(InitialSource.GetLeadingTrivia(), CleanupTypes.Trim_The_File);
            InitialSource = InitialSource.WithLeadingTrivia(newLeadingTriviaList);

            var endOfFileToken = InitialSource.DescendantTokens().SingleOrDefault(x => x.IsKind(SyntaxKind.EndOfFileToken));
            var beforeEndOfFileToken = endOfFileToken.GetPreviousToken();

            var leadingTriviList = endOfFileToken.LeadingTrivia;
            leadingTriviList = CleanUpListWithNoWhitespaces(leadingTriviList, CleanupTypes.Trim_The_File, itsForCloseBrace: true);

            if (CheckOption((int)CleanupTypes.Trim_The_File))
            {
                if (leadingTriviList.Any() && leadingTriviList.Last().IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    leadingTriviList = leadingTriviList.Take(leadingTriviList.Count - 1).ToSyntaxTriviaList();
                }
            }
            var newEndOfFileToken = endOfFileToken.WithLeadingTrivia(leadingTriviList);

            newLeadingTriviaList = CleanUpListWithNoWhitespaces(beforeEndOfFileToken.TrailingTrivia, CleanupTypes.Trim_The_File);
            var newBeforeEndOfFileToken = beforeEndOfFileToken.WithTrailingTrivia(newLeadingTriviaList);

            InitialSource = InitialSource.ReplaceTokens(new[] { endOfFileToken, beforeEndOfFileToken },
                (token1, token2) =>
                {
                    if (token1 == endOfFileToken) return newEndOfFileToken;
                    return newBeforeEndOfFileToken;
                }
                );
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null) return base.Visit(node);

            var nodeLeadingTriviList = node.GetLeadingTrivia();

            if (node is UsingDirectiveSyntax)
            {
                nodeLeadingTriviList = CleanUpListWithNoWhitespaces(nodeLeadingTriviList, CleanupTypes.Remove_DBL_Inside_Usings);
                node = node.WithLeadingTrivia(nodeLeadingTriviList);

                _LastMember = null;
            }
            else if (node is NamespaceDeclarationSyntax)
            {
                node = ApplyNodeChange(node as NamespaceDeclarationSyntax);
            }
            else if (node is ClassDeclarationSyntax)
            {
                node = ApplyNodeChange(node as ClassDeclarationSyntax);
            }
            else if (node is StructDeclarationSyntax)
            {
                node = ApplyNodeChange(node as StructDeclarationSyntax);
            }
            else if (node is MemberDeclarationSyntax)
            {
                if (node is MethodDeclarationSyntax == false || node.Parent is ClassDeclarationSyntax)
                    node = ApplyNodeChange(node as MemberDeclarationSyntax);
            }
            else if (node is BlockSyntax)
            {
                node = ApplyNodeChange(node as BlockSyntax);
            }
            else if (node is StatementSyntax)
            {
                node = ApplyNodeChange(node as StatementSyntax);
            }
            else if (CheckInnerBlocks(node))
            {
                nodeLeadingTriviList = CleanUpListWithDefaultWhitespaces(nodeLeadingTriviList, CleanupTypes.Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets);
                node = node.WithLeadingTrivia(nodeLeadingTriviList);
            }

            return base.Visit(node);
        }

        BlockSyntax ApplyNodeChange(BlockSyntax mainNode)
        {
            var leadingTriviaList = ApplyOpenBracket(mainNode.OpenBraceToken).LeadingTrivia;

            var firstToken = mainNode.OpenBraceToken.GetNextToken();

            var newCloseBraceToken =
                firstToken == mainNode.CloseBraceToken ?
                    ApplyCloseBracket_OfEmptyBlock(mainNode.CloseBraceToken) :
                    ApplyCloseBracket(mainNode.CloseBraceToken);

            mainNode =
                mainNode
                    .WithCloseBraceToken(newCloseBraceToken)
                    .WithLeadingTrivia(leadingTriviaList);

            firstToken = mainNode.OpenBraceToken.GetNextToken();

            mainNode = mainNode.ReplaceToken(firstToken, ApplyOpenBracket(firstToken));

            return mainNode;
        }

        MemberDeclarationSyntax ApplyNodeChange(MemberDeclarationSyntax statementNode)
        {
            var leadingTriviaList = statementNode.GetLeadingTrivia();
            bool isCleanupDone = false;
            if (_LastMember is MethodDeclarationSyntax && IsStartWithSpecialDirective(leadingTriviaList) == false)
            {
                if (CheckOption((int)CleanupTypes.Adding_Blank_after_Method_Close_Bracket))
                {
                    if (leadingTriviaList.Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) < 2)
                    {
                        leadingTriviaList = CleanUpListWithExactNumberOfWhitespaces(leadingTriviaList, 1, null);
                        isCleanupDone = true;
                    }
                }
                else if (CheckOption((int)CleanupTypes.Remove_DBL_Between_Class_Members))
                {
                    leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, null);
                    isCleanupDone = true;
                }
            }
            else if (CheckOption((int)CleanupTypes.Remove_DBL_Between_Class_Members))
            {
                leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, null);
                isCleanupDone = true;
            }

            if (!isCleanupDone)
            {
                leadingTriviaList = ProcessSpecialTrivias(leadingTriviaList, false);
            }

            statementNode = statementNode.WithLeadingTrivia(leadingTriviaList);

            _LastMember = statementNode;

            return statementNode;
        }

        private bool IsStartWithSpecialDirective(SyntaxTriviaList leadingTriviaList)
        {
            var firstDirective = leadingTriviaList.SkipWhile(x => x.IsWhitespaceTrivia()).FirstOrDefault();

            if (firstDirective == default(SyntaxTrivia)) return false;

            if (firstDirective.IsDirective)
            {
                return
                    firstDirective.IsKind(SyntaxKind.ElseDirectiveTrivia) ||
                    firstDirective.IsKind(SyntaxKind.EndIfDirectiveTrivia);
            }
            return false;
        }

        StatementSyntax ApplyNodeChange(StatementSyntax statementNode)
        {
            bool isCleanupDone = false;
            var leadingTriviaList = statementNode.GetLeadingTrivia();

            if (_lastTokenIsACloseBrace)
            {
                if (CheckOption((int)CleanupTypes.Adding_Blank_after_Block_Close_Bracket))
                {
                    if (leadingTriviaList.Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) < 2)
                    {
                        leadingTriviaList = CleanUpListWithExactNumberOfWhitespaces(leadingTriviaList, 1, null);
                        isCleanupDone = true;
                    }
                }
                else if (CheckOption((int)CleanupTypes.Remove_DBL_Between_Methods_Statements))
                {
                    leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, null);
                    isCleanupDone = true;
                }
            }
            else if (CheckOption((int)CleanupTypes.Remove_DBL_Between_Methods_Statements))
            {
                leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, null);
                isCleanupDone = true;
            }

            if (!isCleanupDone)
            {
                leadingTriviaList = ProcessSpecialTrivias(leadingTriviaList, false);
            }

            statementNode = statementNode.WithLeadingTrivia(leadingTriviaList);

            _LastMember = null;

            return statementNode;
        }

        StructDeclarationSyntax ApplyNodeChange(StructDeclarationSyntax mainNode)
        {
            var leadingTriviaList = mainNode.GetLeadingTrivia();

            leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, CleanupTypes.Remove_DBL_Between_Namespace_Members);

            mainNode =
                mainNode
                    .WithOpenBraceToken(ApplyOpenBracket(mainNode.OpenBraceToken))
                    .WithCloseBraceToken(ApplyCloseBracket(mainNode.CloseBraceToken))
                    .WithLeadingTrivia(leadingTriviaList);

            var firstToken = mainNode.OpenBraceToken.GetNextToken();

            mainNode = mainNode.ReplaceToken(firstToken, ApplyOpenBracket(firstToken));

            return mainNode;
        }

        ClassDeclarationSyntax ApplyNodeChange(ClassDeclarationSyntax mainNode)
        {
            var leadingTriviaList = mainNode.GetLeadingTrivia();

            leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, CleanupTypes.Remove_DBL_Between_Namespace_Members);

            mainNode =
                mainNode
                    .WithOpenBraceToken(ApplyOpenBracket(mainNode.OpenBraceToken))
                    .WithCloseBraceToken(ApplyCloseBracket(mainNode.CloseBraceToken))
                    .WithLeadingTrivia(leadingTriviaList);

            var firstToken = mainNode.OpenBraceToken.GetNextToken();

            mainNode = mainNode.ReplaceToken(firstToken, ApplyOpenBracket(firstToken));

            _LastMember = null;

            return mainNode;
        }

        NamespaceDeclarationSyntax ApplyNodeChange(NamespaceDeclarationSyntax mainNode)
        {
            var leadingTriviaList = mainNode.GetLeadingTrivia();

            leadingTriviaList = CleanUpListWithDefaultWhitespaces(leadingTriviaList, CleanupTypes.Remove_DBL_Between_Namespace_Members);

            mainNode =
                mainNode
                    .WithOpenBraceToken(ApplyOpenBracket(mainNode.OpenBraceToken))
                    .WithCloseBraceToken(ApplyCloseBracket(mainNode.CloseBraceToken))
                    .WithLeadingTrivia(leadingTriviaList);

            var firstToken = mainNode.OpenBraceToken.GetNextToken();

            mainNode = mainNode.ReplaceToken(firstToken, ApplyOpenBracket(firstToken));

            return mainNode;
        }

        SyntaxToken ApplyOpenBracket(SyntaxToken openBraceToken)
        {
            var x =
                openBraceToken
                    .WithLeadingTrivia(
                        CleanUpListWithExactNumberOfWhitespaces(openBraceToken.LeadingTrivia, 0, CleanupTypes.Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets, itsForCloseBrace: false));

            return x;
        }

        SyntaxToken ApplyCloseBracket(SyntaxToken closeBraceToken)
        {
            return
                closeBraceToken
                    .WithLeadingTrivia(
                        CleanUpListWithNoWhitespaces(closeBraceToken.LeadingTrivia, CleanupTypes.Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets, itsForCloseBrace: true));
        }
        SyntaxToken ApplyCloseBracket_OfEmptyBlock(SyntaxToken closeBraceToken)
        {
            return
                closeBraceToken
                    .WithLeadingTrivia(
                        CleanUpListWithExactNumberOfWhitespaces(closeBraceToken.LeadingTrivia, 0, CleanupTypes.Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets, itsForCloseBrace: true));
        }

        bool CheckInnerBlocks(SyntaxNode node)
        {
            if (node is CatchClauseSyntax) return true;
            if (node is FinallyClauseSyntax) return true;
            if (node is ElseClauseSyntax) return true;

            return false;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (default(SyntaxToken) == token) return base.VisitToken(token);

            _lastToken = token;

            var tokenKind = token.Kind();

            _lastTokenIsAOpenBrace = tokenKind == SyntaxKind.OpenBraceToken;
            _lastTokenIsACloseBrace = false;

            if (tokenKind == SyntaxKind.CloseBraceToken)
            {
                var triviasBetweenTokens = token.GetPreviousToken().TrailingTrivia.AddRange(token.LeadingTrivia);
                if (triviasBetweenTokens.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                {
                    _lastTokenIsACloseBrace = true;
                }
            }

            return base.VisitToken(token);
        }

        //SyntaxList<StatementSyntax> ReWriteBlockStatements(SyntaxList<StatementSyntax> blockStatements)
        //{
        //    if (blockStatements.Any() == false) return blockStatements;
        //    var first = blockStatements[0];
        //    var newFirst = first.WithLeadingTrivia(CleanUpListWithExactNumberOfWhitespaces(first.GetLeadingTrivia(), 0, null));
        //    return blockStatements.Replace(first, newFirst);
        //}
    }
}