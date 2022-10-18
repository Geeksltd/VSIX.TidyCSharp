using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.TokenRemovers;

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

        // TODO: 1. Fix the issue with touching the namespaces
        // TODO: 2. Remove the conditional operator 
        return properties.Count == 0 ? null : root.RemovePrivateTokens(properties);
    }
}