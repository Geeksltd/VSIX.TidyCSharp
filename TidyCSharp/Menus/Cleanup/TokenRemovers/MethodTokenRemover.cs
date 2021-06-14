using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	public class MethodTokenRemover : CleanupCSharpSyntaxRewriter, IPrivateModiferTokenRemover
	{
		public MethodTokenRemover(bool isReportOnlyMode)
				: base(isReportOnlyMode, null)
		{ }
		public SyntaxNode Remove(SyntaxNode root)
		{
			var methods = new MethodExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);
			if (IsReportOnlyMode)
			{
				foreach (var method in methods)
				{
					var lineSpan = method.GetFileLinePosSpan();
					AddReport(new ChangesReport(root)
					{
						LineNumber = lineSpan.StartLinePosition.Line,
						Column = lineSpan.StartLinePosition.Character,
						Message = "private method --> private can be removed",
						Generator = nameof(MethodTokenRemover)
					});
				}
			}
			// TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
			return methods.Count == 0 ? null : root.RemovePrivateTokens(methods);
		}
	}
}