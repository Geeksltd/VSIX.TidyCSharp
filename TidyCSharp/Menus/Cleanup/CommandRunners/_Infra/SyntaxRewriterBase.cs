using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public abstract class SyntaxRewriterBase : CSharpSyntaxRewriter
    {
        protected IEnumerable<ChangesReport> ChangesReport;
        protected bool IsReportOnlyMode;
        public SyntaxRewriterBase(bool isReportOnlyMode)
        {
            IsReportOnlyMode = isReportOnlyMode;
            ChangesReport = new List<ChangesReport>();
        }
        public virtual IEnumerable<ChangesReport> GetReport()
        {
            return ChangesReport;
        }
    }

    public class ChangesReport
    {
        public string Message { get; set; }
        public long LineNumber { get; set; }
        public string FileName { get; set; }
        public string Generator { get; set; }
        public long Column { get; set; }
    }
}
