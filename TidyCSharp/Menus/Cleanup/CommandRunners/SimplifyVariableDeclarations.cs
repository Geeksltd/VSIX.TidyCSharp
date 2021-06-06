using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
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
			return SimplifyVariableDeclarationsHelper(ProjectItemDetails, IsReportOnlyMode, Options);
		}

		public static SyntaxNode SimplifyVariableDeclarationsHelper(ProjectItemDetailsType projectItemDetails, bool isReportOnlyMode, ICleanupOption options)
		{
			var initialSourceNode = new Rewriter(projectItemDetails, isReportOnlyMode, options).Visit(projectItemDetails.InitialSourceNode);
			return initialSourceNode;
		}

		class Rewriter : CleanupCSharpSyntaxRewriter
		{
			const string VarKeyword = "var";
			readonly ProjectItemDetailsType projectItemDetails;

			public Rewriter(ProjectItemDetailsType projectItemDetails, bool isReportOnlyMode, ICleanupOption options) : base(isReportOnlyMode, options)
			{
				this.projectItemDetails = projectItemDetails;
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

				var typeOfInitializer = projectItemDetails.SemanticModel.GetTypeInfo(variable.Initializer.Value);

				var typeOfTypeDef = projectItemDetails.SemanticModel.GetTypeInfo(node.Type);

				if (typeOfInitializer.Type == typeOfTypeDef.Type)
				{
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