using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils
{
	public abstract class CleanupCSharpSyntaxRewriter : CSharpSyntaxRewriter
	{
		protected ICleanupOption Options { get; private set; }
		protected IEnumerable<ChangesReport> ChangesReport;
		protected bool IsReportOnlyMode;

		protected bool CheckOption(int? o) => Options.Should(o);

		public CleanupCSharpSyntaxRewriter(bool isReportOnlyMode, ICleanupOption options)
		{
			IsReportOnlyMode = isReportOnlyMode;
			ChangesReport = new List<ChangesReport>();
			Options = options;
		}
		public virtual ChangesReport[] GetReport()
		{
			return ChangesReport.ToArray();
		}
	}

	public class ChangesReport
	{
		public ChangesReport(SyntaxNode node)
		{
			FileName = node.GetFilePath();
		}
		public string Message { get; set; }
		public long LineNumber { get; set; }
		public string FileName { get; private set; }
		public string Generator { get; set; }
		public long Column { get; set; }
	}
}