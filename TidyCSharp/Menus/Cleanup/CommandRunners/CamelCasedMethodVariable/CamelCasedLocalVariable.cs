using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class CamelCasedLocalVariable : VariableRenamingBase, ICodeCleaner
	{
		protected override SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes)
		{
			return
				initialSourceNode
					.DescendantNodes()
					.OfType<MethodDeclarationSyntax>()
					.FirstOrDefault(m => !m.HasAnnotation(annotationForSelectedNodes));
		}

		protected override VariableRenamingBaseRewriter GetRewriter(Document workingDocument)
		{
			return new Rewriter(workingDocument, IsReportOnlyMode, Options);
		}
		class Rewriter : VariableRenamingBaseRewriter
		{
			public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

			public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				return base.VisitMethodDeclaration(RenameDeclarations(node) as MethodDeclarationSyntax);
			}

			MethodDeclarationSyntax RenameDeclarations(MethodDeclarationSyntax methodNode)
			{
				if (CheckOption((int)CamelCasedMethodVariable.CleanupTypes.Local_variable))
				{
					var renamingResult = new VariableRenamer(WorkingDocument).RenameDeclarations(methodNode);
					if (renamingResult != null && renamingResult.Node != null)
					{
						methodNode = renamingResult.Node as MethodDeclarationSyntax;
						WorkingDocument = renamingResult.Document;
					}
				}

				if (CheckOption((int)CamelCasedMethodVariable.CleanupTypes.Method_Parameter))
				{
					var renamingResult = new ParameterRenamer(WorkingDocument).RenameDeclarations(methodNode);
					if (renamingResult != null && renamingResult.Node != null)
					{
						methodNode = renamingResult.Node as MethodDeclarationSyntax;
						WorkingDocument = renamingResult.Document;
					}
				}

				return methodNode;
			}
		}
	}
}