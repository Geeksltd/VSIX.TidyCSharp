using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace.Option;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace;

public class EndOfLineRewriter : CSharpSyntaxRewriterBase
{
    public EndOfLineRewriter(SyntaxNode initialSource,
        bool isReadOnlyMode, Options options)
        : base(initialSource, isReadOnlyMode, options)
    { }

    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToFullString() == "\r\n")
        {
            if (IsReportOnlyMode)
            {
                var lineSpan = trivia.GetFileLinePosSpan();

                AddReport(new ChangesReport(trivia)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "\\r\\n should be \\n",
                    Generator = nameof(EndOfLineRewriter)
                });
            }

            return base.VisitTrivia(SyntaxFactory.EndOfLine("\n"));
        }

        return base.VisitTrivia(trivia);
    }
}