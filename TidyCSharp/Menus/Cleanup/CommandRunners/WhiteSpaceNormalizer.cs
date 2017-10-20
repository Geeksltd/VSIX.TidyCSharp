using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return NormalizeWhiteSpaceHelper(initialSourceNode);
        }

        public static SyntaxNode NormalizeWhiteSpaceHelper(SyntaxNode initialSourceNode)
        {
            if (GeeksProductivityToolsPackage.Instance != null)
            {
                initialSourceNode = Formatter.Format(initialSourceNode, GeeksProductivityToolsPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            initialSourceNode = new BlockRewriter(initialSourceNode).Visit(initialSourceNode);
            initialSourceNode = new WhitespaceRewriter(initialSourceNode).Visit(initialSourceNode);
            return initialSourceNode;
        }

        static SyntaxTrivia _endOfLineTrivia = default(SyntaxTrivia);
        public const int BLOCK_SINGLE_STATEMENT_MAX_LENGTH = 70;

        class CSharpSyntaxRewriterBase : CSharpSyntaxRewriter
        {
            public CSharpSyntaxRewriterBase(SyntaxNode initialSource) : base()
            {
                _endOfLineTrivia =
                    initialSource
                        .SyntaxTree
                        .GetRoot()
                        .DescendantTrivia(descendIntoTrivia: true)
                        .FirstOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia));
            }
            #region

            protected SyntaxTriviaList CleanUpList(SyntaxTriviaList newList)
            {
                var lineBreaksAtBeginning = newList.TakeWhile(t => t.IsKind(SyntaxKind.EndOfLineTrivia)).Count();

                if (lineBreaksAtBeginning > 1)
                {
                    newList = newList.Skip(lineBreaksAtBeginning - 1).ToSyntaxTriviaList();
                }

                return newList;
            }

            protected SyntaxTriviaList CleanUpList(SyntaxTriviaList syntaxTrivias, int exactNumberOfBlanks)
            {
                var lineBreaksAtBeginning = syntaxTrivias.TakeWhile(t => t.IsKind(SyntaxKind.EndOfLineTrivia)).Count();

                if (lineBreaksAtBeginning > exactNumberOfBlanks)
                {
                    syntaxTrivias = syntaxTrivias.Skip(lineBreaksAtBeginning - exactNumberOfBlanks)
                        .ToSyntaxTriviaList();
                }
                else if (lineBreaksAtBeginning < exactNumberOfBlanks)
                {
                    var newList = syntaxTrivias.ToList();
                    for (var i = lineBreaksAtBeginning; i < exactNumberOfBlanks; i++)
                    {
                        newList.Insert(0, _endOfLineTrivia);
                    }
                    syntaxTrivias = new SyntaxTriviaList().AddRange(newList);
                }

                return syntaxTrivias;
            }

            protected SyntaxTriviaList CleanUpListWithNoWhitespaces(SyntaxTriviaList syntaxTrivias, bool itsForCloseBrace = false)
            {
                syntaxTrivias = ProcessSpecialTrivias(syntaxTrivias, itsForCloseBrace);

                var specialTriviasCount =
                    syntaxTrivias
                        .Count(t =>
                            !t.IsKind(SyntaxKind.EndOfLineTrivia) && !t.IsKind(SyntaxKind.WhitespaceTrivia)
                        );

                if (specialTriviasCount > 0) return CleanUpList(syntaxTrivias);

                return CleanUpList(syntaxTrivias, 0);
            }

            protected SyntaxTriviaList CleanUpListWithDefaultWhitespaces(SyntaxTriviaList syntaxTrivias, bool itsForCloseBrace = false)
            {
                syntaxTrivias = CleanUpList(syntaxTrivias);
                syntaxTrivias = ProcessSpecialTrivias(syntaxTrivias, itsForCloseBrace);

                return syntaxTrivias;
            }

            protected SyntaxTriviaList CleanUpListWithExactNumberOfWhitespaces(SyntaxTriviaList syntaxTrivias, int exactNumberOfBlanks, bool itsForCloseBrace = false)
            {
                syntaxTrivias = CleanUpList(syntaxTrivias, exactNumberOfBlanks);
                syntaxTrivias = ProcessSpecialTrivias(syntaxTrivias, itsForCloseBrace);

                return syntaxTrivias;
            }

            protected SyntaxTriviaList ProcessSpecialTrivias(SyntaxTriviaList syntaxTrivias, bool itsForCloseBrace)
            {
                if (CheckShortSyntax(syntaxTrivias, itsForCloseBrace)) return syntaxTrivias;
                var specialTriviasCount = syntaxTrivias.Count(t => !t.IsKind(SyntaxKind.EndOfLineTrivia) && !t.IsKind(SyntaxKind.WhitespaceTrivia));

                var outputTriviasList = new List<SyntaxTrivia>();
                var specialTiviasCount = 0;
                var bAddedBlankLine = false;

                for (var i = 0; i < syntaxTrivias.Count; i++)
                {
                    var countOfChars = 0;

                    if (specialTiviasCount == specialTriviasCount)
                    {
                        if (itsForCloseBrace)
                        {
                            i += RemoveBlankDuplication(syntaxTrivias, SyntaxKind.EndOfLineTrivia, i) + 1;

                            if (RemoveBlankDuplication(syntaxTrivias, SyntaxKind.WhitespaceTrivia, i) != -1)
                            {
                                outputTriviasList.Add(syntaxTrivias[i]);
                            }
                            i = syntaxTrivias.Count;
                            continue;
                        }
                    }
                    if
                    (
                        (
                            syntaxTrivias[i].IsKind(SyntaxKind.EndOfLineTrivia) ||
                            syntaxTrivias[i].IsKind(SyntaxKind.WhitespaceTrivia) ||
                            syntaxTrivias[i].IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                            syntaxTrivias[i].IsKind(SyntaxKind.MultiLineCommentTrivia)
                        ) == false
                    )
                    {
                        outputTriviasList.Add(syntaxTrivias[i]);
                        specialTiviasCount++;
                        continue;
                    }

                    if (syntaxTrivias[i].IsKind(SyntaxKind.SingleLineCommentTrivia) || syntaxTrivias[i].IsKind(SyntaxKind.MultiLineCommentTrivia))
                    {
                        if (syntaxTrivias[i].IsKind(SyntaxKind.SingleLineCommentTrivia))
                        {
                            var commentText = syntaxTrivias[i].ToFullString().Trim();
                            if (commentText.Length > 2 && commentText[2] != ' ')
                            {
                                commentText = $"{commentText.Substring(0, 2)} {commentText.Substring(2)}";
                            }
                            syntaxTrivias = syntaxTrivias.Replace(syntaxTrivias[i], SyntaxFactory.Comment(commentText));
                        }
                        outputTriviasList.Add(syntaxTrivias[i]);
                        i++;
                        if (i < syntaxTrivias.Count && syntaxTrivias[i].IsKind(SyntaxKind.EndOfLineTrivia))
                        {
                            outputTriviasList.Add(syntaxTrivias[i]);
                        }
                        specialTiviasCount++;
                        continue;
                    }

                    if ((countOfChars = RemoveBlankDuplication(syntaxTrivias, SyntaxKind.EndOfLineTrivia, i)) != -1)
                    {
                        outputTriviasList.Add(syntaxTrivias[i]);
                        i += countOfChars + 1;
                        bAddedBlankLine = true;
                    }
                    if ((countOfChars = RemoveBlankDuplication(syntaxTrivias, SyntaxKind.WhitespaceTrivia, i)) != -1)
                    {
                        outputTriviasList.Add(syntaxTrivias[i]);
                        i += countOfChars;
                    }
                    else if (bAddedBlankLine)
                    {
                        i--;
                    }
                    bAddedBlankLine = false;
                }
                return outputTriviasList.ToSyntaxTriviaList();
            }
            bool CheckShortSyntax(SyntaxTriviaList syntaxTrivias, bool itsForCloseBrace)
            {
                if (itsForCloseBrace) return false;
                if (syntaxTrivias.Count <= 1) return true;
                if (syntaxTrivias.Count > 2) return false;

                if (syntaxTrivias[0].IsKind(SyntaxKind.EndOfLineTrivia) &&
                    syntaxTrivias[1].IsKind(SyntaxKind.WhitespaceTrivia))
                    return true;
                if (syntaxTrivias[0].IsKind(SyntaxKind.WhitespaceTrivia) &&
                    syntaxTrivias[1].IsKind(SyntaxKind.EndOfLineTrivia))
                    return true;

                return false;
            }
            int RemoveBlankDuplication(SyntaxTriviaList syntaxTrivias, SyntaxKind kind, int iterationIndex)
            {
                if (iterationIndex >= syntaxTrivias.Count) return -1;

                var lineBreaksAtBeginning = syntaxTrivias.Skip(iterationIndex).TakeWhile(t => t.IsKind(kind)).Count();

                return lineBreaksAtBeginning - 1;
            }

            #endregion
        }

        class BlockRewriter : CSharpSyntaxRewriterBase
        {
            public BlockRewriter(SyntaxNode initialSource) : base(initialSource) { }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node is BlockSyntax)
                {
                    var newNode = ApplyNodeChange(node as BlockSyntax);
                    return base.Visit(newNode);
                }
                return base.Visit(node);
            }
            SyntaxToken lastBlockToken = default(SyntaxToken);
            SyntaxNode ApplyNodeChange(BlockSyntax blockNode)
            {
                if (blockNode.Parent is ConversionOperatorDeclarationSyntax) return blockNode;
                if (blockNode.Parent is OperatorDeclarationSyntax) return blockNode;
                if (blockNode.Parent is AccessorDeclarationSyntax) return blockNode;
                if (blockNode.Parent is ConstructorDeclarationSyntax) return blockNode;
                if (blockNode.Parent is DestructorDeclarationSyntax) return blockNode;
                if (blockNode.Parent is TryStatementSyntax) return blockNode;
                if (blockNode.Parent is CatchClauseSyntax) return blockNode;
                if (blockNode.Parent is FinallyClauseSyntax) return blockNode;
                if (blockNode.Parent is IfStatementSyntax) return blockNode;
                if (blockNode.Parent is ElseClauseSyntax) return blockNode;
                //if (blockNode.Parent is ParenthesizedExpressionSyntax) return blockNode;
                if (blockNode.Parent is SimpleLambdaExpressionSyntax) return blockNode;
                if (blockNode.Parent is ParenthesizedLambdaExpressionSyntax) return blockNode;
                if (blockNode.Parent is MethodDeclarationSyntax == false && blockNode.Statements.Count == 1)
                {
                    var singleStatement = blockNode.Statements.First();

                    if (singleStatement.Span.Length <= BLOCK_SINGLE_STATEMENT_MAX_LENGTH)
                    {
                        singleStatement =
                          singleStatement
                              .WithTrailingTrivia(
                                  CleanUpList(
                                      SyntaxFactory.TriviaList(
                                          singleStatement
                                              .GetTrailingTrivia()
                                              .AddRange(blockNode.GetTrailingTrivia())
                                      ), 1)
                              );
                        lastBlockToken = singleStatement.GetLastToken();

                        return singleStatement;
                    }
                }

                return blockNode;
            }

            SyntaxToken lastToken = default(SyntaxToken);
            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                if (lastBlockToken != default(SyntaxToken) && lastBlockToken == lastToken)
                {
                    token = token.WithLeadingTrivia(CleanUpList(token.LeadingTrivia, 1));
                    lastBlockToken = default(SyntaxToken);
                }

                lastToken = token;
                return base.VisitToken(token);
            }
        }

        class WhitespaceRewriter : CSharpSyntaxRewriterBase
        {
            SyntaxToken _lastToken = default(SyntaxToken);
            MemberDeclarationSyntax _LastMember = null;
            bool _lastTokenIsAOpenBrace = false;
            bool _lastTokenIsACloseBrace = false;
            public WhitespaceRewriter(SyntaxNode initialSource) : base(initialSource) { }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null) return base.Visit(node);

                var triviList = node.GetLeadingTrivia();

                if (node is UsingDirectiveSyntax)
                {
                    triviList = CleanUpListWithNoWhitespaces(triviList);
                    node = node.WithLeadingTrivia(triviList);
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
                else if (node is MethodDeclarationSyntax && node.Parent is ClassDeclarationSyntax)
                {
                    node = ApplyNodeChange(node as MethodDeclarationSyntax);
                }
                else if (node is MemberDeclarationSyntax)
                {
                    node = ApplyNodeChange(node as MemberDeclarationSyntax);
                    _LastMember = node as MemberDeclarationSyntax;
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
                    triviList = CleanUpListWithDefaultWhitespaces(triviList);
                    node = node.WithLeadingTrivia(triviList);
                }

                return base.Visit(node);
            }

            BlockSyntax ApplyNodeChange(BlockSyntax blockNode)
            {
                return
                    blockNode
                        .WithOpenBraceToken(
                            blockNode.OpenBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithExactNumberOfWhitespaces(blockNode.OpenBraceToken.LeadingTrivia, 0, itsForCloseBrace: true))
                        )
                        .WithStatements(ReWriteBlockStatements(blockNode.Statements))
                        .WithCloseBraceToken(
                            blockNode.CloseBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithNoWhitespaces(blockNode.CloseBraceToken.LeadingTrivia, itsForCloseBrace: true))
                        );
            }

            SyntaxNode ApplyNodeChange(MethodDeclarationSyntax methodNode)
            {
                var triviList = methodNode.GetLeadingTrivia();

                if (_lastTokenIsAOpenBrace)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 0, itsForCloseBrace: true);
                }
                else if (_LastMember is MethodDeclarationSyntax && IsStartWithSpecialDirective(triviList) == false)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 1);
                }
                else
                {
                    triviList = CleanUpListWithDefaultWhitespaces(triviList);
                }

                _LastMember = methodNode;

                methodNode = methodNode.WithLeadingTrivia(triviList);

                return methodNode;
            }
            MemberDeclarationSyntax ApplyNodeChange(MemberDeclarationSyntax statementNode)
            {
                var triviList = statementNode.GetLeadingTrivia();

                var zeroCondition = _lastTokenIsAOpenBrace || _lastToken == default(SyntaxToken);

                if (zeroCondition)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 0);
                }
                else if (_lastTokenIsACloseBrace && IsStartWithSpecialDirective(triviList) == false)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 1, itsForCloseBrace: false);
                }
                else
                {
                    triviList = CleanUpListWithDefaultWhitespaces(triviList);
                }

                statementNode = statementNode.WithLeadingTrivia(triviList);

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
                var triviList = statementNode.GetLeadingTrivia();

                var zeroCondition = _lastTokenIsAOpenBrace || _lastToken == default(SyntaxToken);

                if (zeroCondition)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 0);
                }
                else if (_lastTokenIsACloseBrace)
                {
                    triviList = CleanUpListWithExactNumberOfWhitespaces(triviList, 1, itsForCloseBrace: false);
                }
                else
                {
                    triviList = CleanUpListWithDefaultWhitespaces(triviList);
                }

                statementNode = statementNode.WithLeadingTrivia(triviList);

                return statementNode;
            }

            StructDeclarationSyntax ApplyNodeChange(StructDeclarationSyntax structNode)
            {
                var triviList = structNode.GetLeadingTrivia();

                structNode =
                    structNode
                        .WithOpenBraceToken(
                            structNode.OpenBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithExactNumberOfWhitespaces(structNode.OpenBraceToken.LeadingTrivia, 0, itsForCloseBrace: true)))
                        .WithCloseBraceToken(
                            structNode.CloseBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithNoWhitespaces(structNode.CloseBraceToken.LeadingTrivia, itsForCloseBrace: true))
                        );

                var zeroCondition = _lastTokenIsAOpenBrace || _lastToken == default(SyntaxToken);
                triviList = CleanUpList(triviList, zeroCondition ? 0 : 1);
                _LastMember = null;

                structNode = structNode.WithLeadingTrivia(triviList);

                return structNode;
            }

            ClassDeclarationSyntax ApplyNodeChange(ClassDeclarationSyntax classNode)
            {
                var triviList = classNode.GetLeadingTrivia();

                classNode =
                    classNode
                        .WithOpenBraceToken(
                            classNode.OpenBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithExactNumberOfWhitespaces(classNode.OpenBraceToken.LeadingTrivia, 0, itsForCloseBrace: true)))
                        .WithCloseBraceToken(
                            classNode.CloseBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithNoWhitespaces(classNode.CloseBraceToken.LeadingTrivia, itsForCloseBrace: true))
                        );


                var zeroCondition = _lastTokenIsAOpenBrace || _lastToken == default(SyntaxToken);
                triviList = CleanUpList(triviList, zeroCondition ? 0 : 1);
                _LastMember = null;

                classNode = classNode.WithLeadingTrivia(triviList);

                return classNode;
            }

            NamespaceDeclarationSyntax ApplyNodeChange(NamespaceDeclarationSyntax namespaceNode)
            {
                var triviList = namespaceNode.GetLeadingTrivia();

                var zeroCondition = _lastTokenIsAOpenBrace || _lastToken == default(SyntaxToken);
                triviList = CleanUpList(triviList, zeroCondition ? 0 : 1);

                namespaceNode =
                    namespaceNode
                        .WithOpenBraceToken(
                            namespaceNode.OpenBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithExactNumberOfWhitespaces(namespaceNode.OpenBraceToken.LeadingTrivia, 0, itsForCloseBrace: true)))
                        .WithCloseBraceToken(
                            namespaceNode.CloseBraceToken
                                .WithLeadingTrivia(
                                    CleanUpListWithNoWhitespaces(namespaceNode.CloseBraceToken.LeadingTrivia, itsForCloseBrace: true))
                        )
                        .WithLeadingTrivia(triviList);

                _LastMember = null;

                return namespaceNode;
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
                var leadingTriviList = token.LeadingTrivia;

                if (tokenKind == SyntaxKind.CloseBraceToken)
                {
                    var triviasBetweenTokens = token.GetPreviousToken().TrailingTrivia.AddRange(token.LeadingTrivia);
                    if (triviasBetweenTokens.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                    {
                        _lastTokenIsACloseBrace = true;
                    }
                }
                else if (token.IsKind(SyntaxKind.EndOfFileToken))
                {
                    leadingTriviList = ProcessSpecialTrivias(CleanUpList(leadingTriviList), itsForCloseBrace: false);
                    if (token.LeadingTrivia != leadingTriviList)
                    {
                        token = token.WithLeadingTrivia(leadingTriviList);
                    }
                }

                return base.VisitToken(token);
            }

            SyntaxList<StatementSyntax> ReWriteBlockStatements(SyntaxList<StatementSyntax> blockStatements)
            {
                if (blockStatements.Any() == false) return blockStatements;
                var first = blockStatements[0];
                var newFirst = first.WithLeadingTrivia(CleanUpListWithExactNumberOfWhitespaces(first.GetLeadingTrivia(), 0));
                return blockStatements.Replace(first, newFirst);
            }
        }
    }
}