using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class SimplifyVariableDeclarations : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
		{
			var syntaxRewriter = new Rewriter(this.ProjectItemDetails.SemanticModel,
				IsReportOnlyMode, Options);
			var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);
			if (IsReportOnlyMode)
			{
				this.CollectMessages(syntaxRewriter.GetReport());
				return initialSourceNode;
			}
			return modifiedSyntaxNode;
		}

		class Rewriter : CleanupCSharpSyntaxRewriter
		{
			const string VarKeyword = "var";
			SemanticModel SemanticModel;
			public Rewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
				: base(isReportOnlyMode, options)
			{
				SemanticModel = semanticModel;
			}

			public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
			{
				return ConvertToVar(node) ?? node;
			}

			SyntaxNode ConvertToVar(VariableDeclarationSyntax node)
			{
				if (node.Parent is LocalDeclarationStatementSyntax == false) return null;
				if ((node.Parent as LocalDeclarationStatementSyntax).IsConst) return null;
				if (node.Type is IdentifierNameSyntax varIdentifierNameSyntax)
				{
					if (varIdentifierNameSyntax.Identifier.ValueText == VarKeyword) return null;
				}

				if (node.Variables.Count > 1) return null;

				var variable = node.Variables.FirstOrDefault();

				if (variable.Initializer == null) return null;

				var typeOfInitializer = SemanticModel.GetTypeInfo(variable.Initializer.Value);

				var typeOfTypeDef = SemanticModel.GetTypeInfo(node.Type);

				if (typeOfInitializer.Type.Name == typeOfTypeDef.Type.Name)
				{
					if (IsReportOnlyMode)
					{
						var lineSpan = node.GetFileLinePosSpan();
						AddReport(new ChangesReport(node)
						{
							LineNumber = lineSpan.StartLinePosition.Line,
							Column = lineSpan.StartLinePosition.Character,
							Message = "Should Convert To Var",
							Generator = nameof(SimplifyVariableDeclarations)
						});
					}
					node =
						node
						.WithType(
							SyntaxFactory.ParseTypeName(VarKeyword)
							.WithTrailingTrivia(SyntaxFactory.Space)
							.WithLeadingTrivia(node.Type.GetLeadingTrivia())
						);
				}

				return base.VisitVariableDeclaration(node);
			}
		}
	}
}