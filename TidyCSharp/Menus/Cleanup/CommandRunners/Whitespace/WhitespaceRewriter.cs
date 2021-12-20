using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhiteSpace
{
    public class WhiteSpaceRewriter : CSharpSyntaxRewriterBase
    {
        SyntaxToken _lastToken = default(SyntaxToken);
        MemberDeclarationSyntax _LastMember;
        bool _lastTokenIsAOpenBrace, _lastTokenIsACloseBrace;
        public WhiteSpaceRewriter(SyntaxNode initialSource, bool isReadOnlyMode, Options options) :
            base(initialSource, isReadOnlyMode, options)
        { }

        public SyntaxNode Apply()
        {
            TrimFile();
            AddNewLineAfterUsings();

            return Visit(InitialSource);
        }

        void AddNewLineAfterUsings()
        {
            if (CheckOption((int)CleanupTypes.Add_An_Empty_Line_After_Using_Statements))
            {
                var list = InitialSource.DescendantNodesOfType<UsingDirectiveSyntax>()
                .Select(x => new
                {
                    StartLine = x.GetFileLinePosSpan().StartLinePosition.Line,
                    EndLine = x.GetFileLinePosSpan().EndLinePosition.Line,
                    EndPosition = x.FullSpan.End,
                    Directive = x
                });

                var join = list.Join(list, a => a.EndLine + 1, b => b.StartLine, (a, b) => a);

                var selectedDirectives = list.Except(join)
                    .Where(x => !InitialSource.FindTrivia(x.EndPosition, false)
                    .IsKind(SyntaxKind.EndOfLineTrivia))
                    .Select(x => x.Directive);

                InitialSource = InitialSource.ReplaceNodes(selectedDirectives, (a, b) =>
                {
                    return b.WithTrailingTrivia(
                            b.GetTrailingTrivia().AddRange(
                                SyntaxFactory.ParseTrailingTrivia("\n")));
                });
            }
        }

        void TrimFile()
        {
            var newLeadingTriviaList = CleanUpListWithNoWhiteSpaces(InitialSource.GetLeadingTrivia(), CleanupTypes.TrimTheFile);
            InitialSource = InitialSource.WithLeadingTrivia(newLeadingTriviaList);

            var endOfFileToken = InitialSource.DescendantTokens().SingleOrDefault(x => x.IsKind(SyntaxKind.EndOfFileToken));
            var beforeEndOfFileToken = endOfFileToken.GetPreviousToken();

            var leadingTriviList = endOfFileToken.LeadingTrivia;
            leadingTriviList = CleanUpListWithNoWhiteSpaces(leadingTriviList, CleanupTypes.TrimTheFile, itsForCloseBrace: true);

            if (CheckOption((int)CleanupTypes.TrimTheFile))
            {
                if (leadingTriviList.Any() && leadingTriviList.Last().IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    leadingTriviList = leadingTriviList.Take(leadingTriviList.Count - 1).ToSyntaxTriviaList();
                }
            }

            var newEndOfFileToken = endOfFileToken.WithLeadingTrivia(leadingTriviList);

            newLeadingTriviaList = CleanUpListWithNoWhiteSpaces(beforeEndOfFileToken.TrailingTrivia, CleanupTypes.TrimTheFile);

            var newBeforeEndOfFileToken = beforeEndOfFileToken.WithTrailingTrivia(newLeadingTriviaList)
               .WithTrailingTrivia((leadingTriviList != null && leadingTriviList.Any()) ? SyntaxFactory.CarriageReturn : SyntaxFactory.ElasticMarker);

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
            SyntaxNode newNode = null;

            if (node is UsingDirectiveSyntax)
            {
                nodeLeadingTriviList = CleanUpListWithNoWhiteSpaces(nodeLeadingTriviList, CleanupTypes.RemoveDuplicateInsideUsings);
                newNode = node.WithLeadingTrivia(nodeLeadingTriviList);

                _LastMember = null;
            }
            else if (node is NamespaceDeclarationSyntax)
            {
                newNode = ApplyNodeChange(node as NamespaceDeclarationSyntax);
            }
            else if (node is ClassDeclarationSyntax)
            {
                newNode = ApplyNodeChange(node as ClassDeclarationSyntax);
            }
            else if (node is StructDeclarationSyntax)
            {
                newNode = ApplyNodeChange(node as StructDeclarationSyntax);
            }
            else if (node is MemberDeclarationSyntax)
            {
                if (node is MethodDeclarationSyntax == false || node.Parent is ClassDeclarationSyntax)
                    newNode = ApplyNodeChange(node as MemberDeclarationSyntax);
            }
            else if (node is BlockSyntax)
            {
                newNode = ApplyNodeChange(node as BlockSyntax);
            }
            else if (node is StatementSyntax)
            {
                newNode = ApplyNodeChange(node as StatementSyntax);
            }
            else if (CheckInnerBlocks(node))
            {
                nodeLeadingTriviList = CleanUpListWithDefaultWhiteSpaces(nodeLeadingTriviList, CleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets);
                newNode = node.WithLeadingTrivia(nodeLeadingTriviList);
            }

            if (newNode != null)
            {
                if (isReportOnlyMode)
                {
                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Whitespaces should be normalized",
                        Generator = nameof(WhiteSpaceRewriter)
                    });
                }

                return base.Visit(newNode);
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
            var isCleanupDone = false;

            if (_LastMember is MethodDeclarationSyntax && IsStartWithSpecialDirective(leadingTriviaList) == false)
            {
                if (CheckOption((int)CleanupTypes.Adding_Blank_After_Method_Close_Bracket))
                {
                    if (leadingTriviaList.Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) < 2)
                    {
                        leadingTriviaList = CleanUpListWithExactNumberOfWhiteSpaces(leadingTriviaList, 1, null);
                        isCleanupDone = true;
                    }
                }
                else if (CheckOption((int)CleanupTypes.RemoveDuplicateBetweenClassMembers))
                {
                    leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, null);
                    isCleanupDone = true;
                }
            }
            else if (CheckOption((int)CleanupTypes.RemoveDuplicateBetweenClassMembers))
            {
                leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, null);
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

        bool IsStartWithSpecialDirective(SyntaxTriviaList leadingTriviaList)
        {
            var firstDirective = leadingTriviaList.SkipWhile(x => x.IsWhiteSpaceTrivia()).FirstOrDefault();

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
            var isCleanupDone = false;
            var leadingTriviaList = statementNode.GetLeadingTrivia();

            if (statementNode.IsKind(SyntaxKind.EmptyStatement)) // remove unused semicolons
                return null;

            if (_lastTokenIsACloseBrace)
            {
                if (CheckOption((int)CleanupTypes.Adding_Blank_After_Block_Close_Bracket))
                {
                    if (leadingTriviaList.Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) < 2)
                    {
                        leadingTriviaList = CleanUpListWithExactNumberOfWhiteSpaces(leadingTriviaList, 1, null);
                        isCleanupDone = true;
                    }
                }
                else if (CheckOption((int)CleanupTypes.RemoveDuplicateBetweenMethodsStatements))
                {
                    leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, null);
                    isCleanupDone = true;
                }
            }
            else if (CheckOption((int)CleanupTypes.RemoveDuplicateBetweenMethodsStatements))
            {
                leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, null);
                isCleanupDone = true;
            }

            if (!isCleanupDone)
            {
                leadingTriviaList = ProcessSpecialTrivias(leadingTriviaList, false);
            }

            statementNode = statementNode.WithLeadingTrivia(leadingTriviaList);

            // _LastMember = null;

            return statementNode;
        }

        StructDeclarationSyntax ApplyNodeChange(StructDeclarationSyntax mainNode)
        {
            var leadingTriviaList = mainNode.GetLeadingTrivia();

            leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, CleanupTypes.RemoveDuplicateBetweenNamespaceMembers);

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

            leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, CleanupTypes.RemoveDuplicateBetweenNamespaceMembers);

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

            leadingTriviaList = CleanUpListWithDefaultWhiteSpaces(leadingTriviaList, CleanupTypes.RemoveDuplicateBetweenNamespaceMembers);

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
                        CleanUpListWithExactNumberOfWhiteSpaces(openBraceToken.LeadingTrivia, 0, CleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets, itsForCloseBrace: false));

            return x;
        }

        SyntaxToken ApplyCloseBracket(SyntaxToken closeBraceToken)
        {
            return
                closeBraceToken
                    .WithLeadingTrivia(
                        CleanUpListWithNoWhiteSpaces(closeBraceToken.LeadingTrivia, CleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets, itsForCloseBrace: true));
        }

        SyntaxToken ApplyCloseBracket_OfEmptyBlock(SyntaxToken closeBraceToken)
        {
            return
                closeBraceToken
                    .WithLeadingTrivia(
                        CleanUpListWithExactNumberOfWhiteSpaces(closeBraceToken.LeadingTrivia, 0, CleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets, itsForCloseBrace: true));
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

        // SyntaxList<StatementSyntax> ReWriteBlockStatements(SyntaxList<StatementSyntax> blockStatements)
        // {
        //    if (blockStatements.Any() == false) return blockStatements;
        //    var first = blockStatements[0];
        //    var newFirst = first.WithLeadingTrivia(CleanUpListWithExactNumberOfWhitespaces(first.GetLeadingTrivia(), 0, null));
        //    return blockStatements.Replace(first, newFirst);
        // }
    }
}