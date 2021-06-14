using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	public class FieldTokenRemover : CleanupCSharpSyntaxRewriter, IPrivateModiferTokenRemover
	{
		public FieldTokenRemover(bool isReportOnlyMode)
		: base(isReportOnlyMode, null)
		{ }
		public SyntaxNode Remove(SyntaxNode root)
		{
			var fields = new FieldExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);

			if (IsReportOnlyMode)
			{
				foreach (var field in fields)
				{
					var lineSpan = field.GetFileLinePosSpan();
					AddReport(new ChangesReport(root)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "private fields --> private can be removed",
						Generator = nameof(FieldTokenRemover)
					});
				}
			}
			// TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
			return fields.Count == 0 ? null : root.RemovePrivateTokens(fields);
		}
	}
}