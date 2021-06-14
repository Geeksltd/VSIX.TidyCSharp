using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	public class PropertyTokenRemover : CleanupCSharpSyntaxRewriter, IPrivateModiferTokenRemover
	{
		public PropertyTokenRemover(bool isReportOnlyMode)
				: base(isReportOnlyMode, null)
		{ }
		public SyntaxNode Remove(SyntaxNode root)
		{
			var properties = new PropertyExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);

			if (IsReportOnlyMode)
			{
				foreach (var prop in properties)
				{
					var lineSpan = prop.GetFileLinePosSpan();
					AddReport(new ChangesReport(root)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "private property --> private can be removed",
						Generator = nameof(PropertyTokenRemover)
					});
				}
			}
			// TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
			return properties.Count == 0 ? null : root.RemovePrivateTokens(properties);
		}
	}
}