using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
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
		public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
		{
			var rewriter = new Rewriter(IsReportOnlyMode, Options);
			var modifiedSourceNode = rewriter.Visit(initialSourceNode);

			if (IsReportOnlyMode)
			{
				this.CollectMessages(rewriter.GetReport());
				return initialSourceNode;
			}
			return modifiedSourceNode;
			// return SimplyAsyncCallsHelper2(initialSourceNode);
		}
		class Rewriter : CleanupCSharpSyntaxRewriter
		{
			public Rewriter(bool isReportOnlyMode, ICleanupOption options)
				: base(isReportOnlyMode, options)
			{
			}

			public override SyntaxNode Visit(SyntaxNode node)
			{
				if (node == null) return base.Visit(node);
				if (node is MethodDeclarationSyntax == false) return base.Visit(node);
				if (node.Parent is ClassDeclarationSyntax == false) return base.Visit(node);

				var newNode = SimplyAsyncCallsHelper((MethodDeclarationSyntax)node, Options);

				if (!newNode.IsEquivalentTo(node) && IsReportOnlyMode)
				{
					var lineSpan = node.GetFileLinePosSpan();
					AddReport(new ChangesReport(node)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "you can remove await/async modifiers",
						Generator = nameof(SimplyAsyncCalls)
					});
				}

				return base.Visit(newNode);
			}

			static SyntaxTrivia[] _spaceTrivia = { SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ") };

			public SyntaxNode SimplyAsyncCallsHelper(SyntaxNode initialSource, ICleanupOption options)
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

			public MethodDeclarationSyntax SimplyAsyncCallsHelper(MethodDeclarationSyntax method, ICleanupOption options)
			{
				if ((method.Parent is ClassDeclarationSyntax) == false) return method;
				if (method.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword)) == false) return method;
				if (method.Body == null) return method;
				if (method.ReturnType.WithoutTrivia().ToFullString() == typeof(Task).Name) return method;
				if (method.ReturnType.WithoutTrivia().ToFullString() == "void") return method;
				if (method.Body.Statements.Count != 1) return method;

				var singleStatement = method.Body.Statements.FirstOrDefault();

				if (singleStatement.DescendantNodesAndSelf()
					.Count(x => x.IsKind(SyntaxKind.AwaitExpression)) > 1)
					return method;

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
							method.Modifiers.Remove(method.Modifiers.FirstOrDefault(x => x.IsKind(SyntaxKind.AsyncKeyword))))
						.WithLeadingTrivia(method.GetLeadingTrivia())
						.WithTrailingTrivia(method.GetTrailingTrivia());
			}
		}
	}
}