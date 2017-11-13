using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    class BlockRewriter : CSharpSyntaxRewriterBase
    {
        public BlockRewriter(SyntaxNode initialSource, Options options) : base(initialSource, options) { }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (CheckOption((int)CleanupTypes.Remove_Brackets_of_block_that_has_only_one_statement_with_length_shorter_than_70_chars))
            {
                if (node is BlockSyntax)
                {
                    var newNode = ApplyNodeChange(node as BlockSyntax);
                    return base.Visit(newNode);
                }
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
            if (blockNode.Parent is AnonymousMethodExpressionSyntax) return blockNode;
            if (blockNode.Parent is MethodDeclarationSyntax == false && blockNode.Statements.Count == 1)
            {
                var singleStatement = blockNode.Statements.First();

                if (singleStatement.Span.Length <= NormalizeWhitespace.Options.BLOCK_SINGLE_STATEMENT_MAX_LENGTH)
                {
                    singleStatement =
                      singleStatement
                          .WithTrailingTrivia(
                              CleanUpListWithExactNumberOfWhitespaces(
                                  SyntaxFactory.TriviaList(
                                      blockNode.CloseBraceToken.LeadingTrivia
                                      .AddRange(singleStatement.GetTrailingTrivia())
                                      .AddRange(blockNode.GetTrailingTrivia())
                                  ), 1, null)
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
                token = token.WithLeadingTrivia(CleanUpListWithExactNumberOfWhitespaces(token.LeadingTrivia, 1, null));
                lastBlockToken = default(SyntaxToken);
            }

            lastToken = token;
            return base.VisitToken(token);
        }
    }
}
