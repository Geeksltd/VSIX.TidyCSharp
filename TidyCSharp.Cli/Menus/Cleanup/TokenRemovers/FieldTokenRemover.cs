using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.TokenRemovers;

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

        // TODO: 1. Fix the issue with touching the namespaces
        // TODO: 2. Remove the conditional operator 
        return fields.Count == 0 ? null : root.RemovePrivateTokens(fields);
    }
}