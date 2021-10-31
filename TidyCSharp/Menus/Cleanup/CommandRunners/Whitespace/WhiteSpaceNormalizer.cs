using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
		{
			return NormalizeWhiteSpaceHelper(initialSourceNode, Options);
		}

		public Options Options { get; set; }

		public SyntaxNode NormalizeWhiteSpaceHelper(SyntaxNode initialSourceNode, Options options)
		{
			SyntaxNode modifiedSourceNode = initialSourceNode;
			if (TidyCSharpPackage.Instance != null)
			{
				modifiedSourceNode = Formatter.Format(modifiedSourceNode, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
			}

			var blockRewriter = new BlockRewriter(modifiedSourceNode, IsReportOnlyMode, options);
			modifiedSourceNode = blockRewriter.Visit(modifiedSourceNode);

			if (IsReportOnlyMode)
			{
				this.CollectMessages(blockRewriter.GetReport());
			}
			modifiedSourceNode = RefreshResult(modifiedSourceNode);

			if (CheckOption((int)CleanupTypes.Use_slash_n_instead_of_slash_sr_slash_n))
			{
				var endoflineRewriter = new EndOFLineRewriter(modifiedSourceNode, IsReportOnlyMode, options);
				modifiedSourceNode = endoflineRewriter.Visit(modifiedSourceNode);
				modifiedSourceNode = RefreshResult(modifiedSourceNode);

				if (IsReportOnlyMode)
				{
					this.CollectMessages(endoflineRewriter.GetReport());
				}
			}

			var whitespaceRewriter = new WhitespaceRewriter(modifiedSourceNode, IsReportOnlyMode, options);
			modifiedSourceNode = whitespaceRewriter.Apply();

			if (IsReportOnlyMode)
			{
				this.CollectMessages(whitespaceRewriter.GetReport());
			}
			if (CheckOption((int)CleanupTypes.Add_blank_line_between_statements_more_than_one_line))
			{
				modifiedSourceNode = RefreshResult(modifiedSourceNode);
				var blRewriter = new BlankLineRewriter(modifiedSourceNode, IsReportOnlyMode, this.ProjectItemDetails.SemanticModel);
				modifiedSourceNode = blRewriter.Visit(modifiedSourceNode);
				if (IsReportOnlyMode)
				{
					this.CollectMessages(blRewriter.GetReport());
				}
			}
			if (IsReportOnlyMode)
				return initialSourceNode;
			return modifiedSourceNode;
		}
	}
}