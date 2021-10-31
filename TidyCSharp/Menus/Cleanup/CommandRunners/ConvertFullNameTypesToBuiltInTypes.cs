using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class ConvertFullNameTypesToBuiltInTypes : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
		{
			var syntaxRewriter = new Rewriter(this.ProjectItemDetails.SemanticModel, this.IsReportOnlyMode
				, this.Options);
			var modifiedSourceNode = syntaxRewriter.ConvertFullNameTypesToBuiltInTypesHelper(initialSourceNode);
			if (IsReportOnlyMode)
			{
				this.CollectMessages(syntaxRewriter.GetReport());
				return initialSourceNode;
			}
			return modifiedSourceNode;
		}


		class Rewriter : CleanupCSharpSyntaxRewriter
		{
			SemanticModel SemanticModel;
			public Rewriter(SemanticModel semanticModel,
				bool isReportOnlyMode, ICleanupOption options) : base(isReportOnlyMode, options)
			{
				this.SemanticModel = semanticModel;
			}

			public SyntaxNode ConvertFullNameTypesToBuiltInTypesHelper(SyntaxNode initialSource)
			{
				var builtInTypesMapDic = TypesMapItem.GetBuiltInTypesDic();

				var selectedTokensList =
					initialSource
						.DescendantNodes()
						.Where
						(
							n =>
								(n is IdentifierNameSyntax || n is QualifiedNameSyntax)
								&&
								builtInTypesMapDic.ContainsKey(n.WithoutTrivia().ToFullString())
						);
				return initialSource.ReplaceNodes(
						selectedTokensList,
						(oldNode1, oldNode2) =>
						{
							if (oldNode1.Parent is QualifiedNameSyntax) return oldNode1;
							if (oldNode1.Parent is MemberAccessExpressionSyntax)
							{
								if ((oldNode1.Parent as MemberAccessExpressionSyntax).Expression != oldNode1) return oldNode1;
								var symbol = SemanticModel.GetSymbolInfo(oldNode1).Symbol;
								if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
							}
							else if (oldNode1 is IdentifierNameSyntax == false && oldNode1 is QualifiedNameSyntax == false) return oldNode1;
							else
							{
								var symbol = SemanticModel.GetSymbolInfo(oldNode1).Symbol;
								if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
							}

							var lineSpan = oldNode1.GetFileLinePosSpan();
							AddReport(new ChangesReport(oldNode1)
							{
								LineNumber = lineSpan.StartLinePosition.Line,
								Column = lineSpan.StartLinePosition.Character,
								Message = "ConvertFullNameTypesToBuiltInTypes",
								Generator = nameof(ConvertFullNameTypesToBuiltInTypes)
							});

							return
								builtInTypesMapDic[oldNode1.WithoutTrivia().ToFullString()]
									.NewNode
									.WithLeadingTrivia(oldNode1.GetLeadingTrivia())
									.WithTrailingTrivia(oldNode1.GetTrailingTrivia());
						}
					);
			}
		}
	}
}