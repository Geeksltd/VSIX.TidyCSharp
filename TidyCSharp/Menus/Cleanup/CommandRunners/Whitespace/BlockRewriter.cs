using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
	class BlockRewriter : CSharpSyntaxRewriterBase
	{
		public BlockRewriter(SyntaxNode initialSource, bool isReadOnlyMode, Options options)
			: base(initialSource, isReadOnlyMode, options) { }

		public override SyntaxNode Visit(SyntaxNode node)
		{
			if (CheckOption((int)CleanupTypes.Remove_Brackets_of_block_that_has_only_one_statement_with_length_shorter_than_80_chars))
			{
				if (node is BlockSyntax)
				{
					var newNode = ApplyNodeChange(node as BlockSyntax);
					return base.Visit(newNode);
				}
				else if (node is SimpleLambdaExpressionSyntax)
				{
					var newNode = ApplyNodeChange(node as SimpleLambdaExpressionSyntax);
					return base.Visit(newNode);
				}
			}

			return base.Visit(node);
		}

		SyntaxToken lastBlockToken = default(SyntaxToken);
		SyntaxNode ApplyNodeChange(SimpleLambdaExpressionSyntax lambdaNode)
		{
			if (lambdaNode.Block == null) return lambdaNode;
			var blockSyntax = lambdaNode.Block as BlockSyntax;
			if (blockSyntax.Statements.Count == 1 &&
				blockSyntax.Statements.FirstOrDefault().ToString().Length <= 80)
			{
				var newNode = blockSyntax.Statements.FirstOrDefault() as StatementSyntax;

				if (IsReportOnlyMode)
				{
					var lineSpan = lambdaNode.GetFileLinePosSpan();
					AddReport(new ChangesReport(lambdaNode)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "remove brackets from on line lambda expression",
						Generator = nameof(BlockRewriter)
					});
				}

				if (newNode.IsKind(SyntaxKind.ReturnStatement))
				{
					var statementSyntax = newNode as ReturnStatementSyntax;
					if (statementSyntax != null)
					{
						return lambdaNode.WithBlock(null).WithExpressionBody(statementSyntax.Expression);
					}
				}
				else
				{
					var expressionStatement = newNode as ExpressionStatementSyntax;
					if (expressionStatement != null)
					{
						return lambdaNode.WithBlock(null).WithExpressionBody(expressionStatement.Expression.WithoutTrivia()
						.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(" ")))
						.WithArrowToken(lambdaNode.ArrowToken.WithoutTrivia());
					}
				}

				//return lambdaNode.WithBlock(null)
				//	.WithExpressionBody((newNode.IsKind(SyntaxKind.ReturnStatement)
				//	? (newNode as ReturnStatementSyntax).Expression :
				//	(newNode as ExpressionStatementSyntax).Expression)
				//		.WithoutTrivia()
				//		.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(" ")))
				//	.WithArrowToken(lambdaNode.ArrowToken.WithoutTrivia());
			}
			return lambdaNode;
		}
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
			// if (blockNode.Parent is ParenthesizedExpressionSyntax) return blockNode;
			if (blockNode.Parent is SimpleLambdaExpressionSyntax) return blockNode;
			if (blockNode.Parent is ParenthesizedLambdaExpressionSyntax) return blockNode;
			if (blockNode.Parent is AnonymousMethodExpressionSyntax) return blockNode;
			if (blockNode.Parent is LocalFunctionStatementSyntax) return blockNode;
			if (blockNode.Parent is MethodDeclarationSyntax == false && blockNode.Statements.Count == 1)
			{
				var singleStatement = blockNode.Statements.FirstOrDefault();

				if (singleStatement.Span.Length <= NormalizeWhitespace.Options.BLOCK_SINGLE_STATEMENT_MAX_LENGTH)
				{
					if (IsReportOnlyMode)
					{
						var lineSpan = singleStatement.GetFileLinePosSpan();
						AddReport(new ChangesReport(singleStatement)
						{
							LineNumber = lineSpan.StartLinePosition.Line,
							Column = lineSpan.StartLinePosition.Character,
							Message = "remove brackets from on line lambda expression",
							Generator = nameof(BlockRewriter)
						});
					}

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