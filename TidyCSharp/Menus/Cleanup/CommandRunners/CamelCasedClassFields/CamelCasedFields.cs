using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class CamelCasedFields : VariableRenamingBase, ICodeCleaner
	{
		protected override SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes)
		{
			return
				initialSourceNode
					.DescendantNodes()
					.OfType<ClassDeclarationSyntax>()
					.FirstOrDefault(m => !m.HasAnnotation(annotationForSelectedNodes));
		}

		protected override VariableRenamingBaseRewriter GetRewriter(Document workingDocument)
		{
			return new Rewriter(workingDocument, IsReportOnlyMode, Options);
		}

		class Rewriter : VariableRenamingBaseRewriter
		{
			public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

			public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				return base.VisitClassDeclaration(RenameDeclarations(node) as ClassDeclarationSyntax);
			}

			ClassDeclarationSyntax RenameDeclarations(ClassDeclarationSyntax classNode)
			{
				if (CheckOption((int)CamelCasedClassFields.CleanupTypes.Normal_Fields))
				{
					var renamingResult = new FieldRenamer(WorkingDocument).RenameDeclarations(classNode);
					if (renamingResult != null && renamingResult.Node != null)
					{
						classNode = renamingResult.Node as ClassDeclarationSyntax;
						WorkingDocument = renamingResult.Document;
					}
					var lineSpan = classNode.GetFileLinePosSpan();
					AddReport(new ChangesReport(classNode)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "Camel Cased Fields",
						Generator = nameof(CamelCasedFields)
					});
				}

				if (CheckOption((int)CamelCasedClassFields.CleanupTypes.Const_Fields))
				{
					var renamingResult = new CONSTRenamer(WorkingDocument).RenameDeclarations(classNode);
					if (renamingResult != null && renamingResult.Node != null)
					{
						classNode = renamingResult.Node as ClassDeclarationSyntax;
						WorkingDocument = renamingResult.Document;
					}
					var lineSpan = classNode.GetFileLinePosSpan();
					AddReport(new ChangesReport(classNode)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "CamelCasedFields",
						Generator = nameof(CamelCasedFields)
					});
				}

				return classNode;
			}
		}
	}
}