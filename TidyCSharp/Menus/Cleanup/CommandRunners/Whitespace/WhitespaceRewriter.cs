﻿using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
	public class WhitespaceRewriter : CSharpSyntaxRewriterBase
	{
		SyntaxToken _lastToken = default(SyntaxToken);
		MemberDeclarationSyntax _LastMember;
		bool _lastTokenIsAOpenBrace;
		bool _lastTokenIsACloseBrace;
		public WhitespaceRewriter(SyntaxNode initialSource, bool isReadOnlyMode, Options options) :
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
			if (CheckOption((int)CleanupTypes.Add_an_empty_line_after_using_statements))
			{
				var list = InitialSource.DescendantNodesOfType<UsingDirectiveSyntax>()
				.Select(x => new
				{
					StartLine = x.GetFileLinePosSpan().StartLinePosition.Line,
					EndLine = x.GetFileLinePosSpan().EndLinePosition.Line,
					Directive = x
				});

				var selectedDirectives = list.Join(list, a => a.EndLine + 1, b => b.StartLine, (a, b) => b.Directive)
					.Where(x => x.GetTrailingTrivia()
					.Count(y => y.IsKind(SyntaxKind.EndOfLineTrivia)) < 2);

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

			var newBeforeEndOfFileToken = beforeEndOfFileToken.WithTrailingTrivia(newLeadingTriviaList)
			   .WithTrailingTrivia((leadingTriviList != null && leadingTriviList.Any()) ? SyntaxFactory.CarriageReturn : SyntaxFactory.ElasticSpace);

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
				nodeLeadingTriviList = CleanUpListWithNoWhitespaces(nodeLeadingTriviList, CleanupTypes.Remove_DBL_Inside_Usings);
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
				nodeLeadingTriviList = CleanUpListWithDefaultWhitespaces(nodeLeadingTriviList, CleanupTypes.Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets);
				newNode = node.WithLeadingTrivia(nodeLeadingTriviList);
			}
			if (newNode != null)
			{
				if (IsReportOnlyMode)
				{
					var lineSpan = node.GetFileLinePosSpan();
					AddReport(new ChangesReport(node)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "Whitespaces should be normalized",
						Generator = nameof(WhitespaceRewriter)
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

		bool IsStartWithSpecialDirective(SyntaxTriviaList leadingTriviaList)
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

			if (statementNode.IsKind(SyntaxKind.EmptyStatement)) // remove unused semicolons
				return null;

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

			// _LastMember = null;

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

		// SyntaxList<StatementSyntax> ReWriteBlockStatements(SyntaxList<StatementSyntax> blockStatements)
		// {
		//    if (blockStatements.Any() == false) return blockStatements;
		//    var first = blockStatements[0];
		//    var newFirst = first.WithLeadingTrivia(CleanUpListWithExactNumberOfWhitespaces(first.GetLeadingTrivia(), 0, null));
		//    return blockStatements.Replace(first, newFirst);
		// }
	}
}