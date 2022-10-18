using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils;

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