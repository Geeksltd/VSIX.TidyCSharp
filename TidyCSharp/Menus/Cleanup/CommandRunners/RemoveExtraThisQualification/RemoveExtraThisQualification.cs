using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class RemoveExtraThisQualification : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
		{
			return new Rewriter(ProjectItemDetails, Options).Visit(initialSourceNode);
		}

		class Rewriter : CleanupCSharpSyntaxRewriter
		{
			readonly SemanticModel semanticModel;

			public Rewriter(ProjectItemDetailsType projectItemDetails, ICleanupOption options)
				: base(false, options)
			{
				semanticModel = projectItemDetails.SemanticModel;
			}

			public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				if (node.Parent is ClassDeclarationSyntax == false)
				{
					node = Remove(node);
				}

				return base.VisitClassDeclaration(node);
			}

			ClassDeclarationSyntax Remove(ClassDeclarationSyntax classNode)
			{
				var thises = classNode.DescendantNodes().OfType<ThisExpressionSyntax>();
				var newItems = new Dictionary<MemberAccessExpressionSyntax, SyntaxNode>();

				foreach (var thisItem in thises)
				{
					if (thisItem.Parent is MemberAccessExpressionSyntax thisItemAsMemberAccessException)
					{
						var newAccess = GetMemberAccessWithoutThis(thisItemAsMemberAccessException);

						if (newAccess != null)
						{
							newItems.Add(thisItemAsMemberAccessException, newAccess);
						}
					}
				}

				if (newItems.Any())
				{
					classNode = classNode.ReplaceNodes(newItems.Keys, (node1, node2) => newItems[node1]);
				}

				return classNode;
			}

			SyntaxNode GetMemberAccessWithoutThis(MemberAccessExpressionSyntax thisItemAsMemberAccessException)
			{
				var thisItemAsMemberAccessExceptionSymbol = semanticModel.GetSymbolInfo(thisItemAsMemberAccessException).Symbol;

				if (thisItemAsMemberAccessExceptionSymbol is IFieldSymbol && !CheckOption((int)RemoveExtraThisKeyword.CleanupTypes.Remove_From_Fields_Call)) return null;
				if (thisItemAsMemberAccessExceptionSymbol is IPropertySymbol && !CheckOption((int)RemoveExtraThisKeyword.CleanupTypes.Remove_From_Properties_Call)) return null;
				if (thisItemAsMemberAccessExceptionSymbol is IMethodSymbol && !CheckOption((int)RemoveExtraThisKeyword.CleanupTypes.Remove_From_Method_Call)) return null;

				var right = thisItemAsMemberAccessException.Name;
				var symbols = semanticModel.LookupSymbols(thisItemAsMemberAccessException.SpanStart, name: right.Identifier.ValueText);
				if (symbols.Any(x => x == thisItemAsMemberAccessExceptionSymbol))
				{
					return right.WithLeadingTrivia(thisItemAsMemberAccessException.GetLeadingTrivia());
				}

				return null;
			}
		}
	}
}