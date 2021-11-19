using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
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
        public virtual ChangesReport[] GetReport() => ChangesReport.ToArray();

        public void AddReport(ChangesReport changesReports)
        {
            if (changesReports.FileName.HasValue())
                ChangesReport = ChangesReport.Append(changesReports);
        }
    }

    public class ChangesReport
    {
        public ChangesReport(SyntaxNode node) => FileName = node.GetFilePath();

        public ChangesReport(SyntaxTrivia trivia) => FileName = trivia.GetFilePath();
        public string Message { get; set; }
        public long LineNumber { get; set; }
        public string FileName { get; private set; }
        public string Generator { get; set; }
        public long Column { get; set; }
    }
}