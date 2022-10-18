using Microsoft.CodeAnalysis.CSharp;
using TidyCSharp.Cli.Extensions;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils;

public abstract class CleanupCSharpSyntaxRewriter : CSharpSyntaxRewriter
{
    protected ICleanupOption Options { get; private set; }
    protected IEnumerable<ChangesReport> ChangesReport { get; set; }
    protected bool IsReportOnlyMode { get; set; }

    protected bool CheckOption(int? o) => Options.Should(o);

    protected CleanupCSharpSyntaxRewriter(bool isReportOnlyMode, ICleanupOption options)
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