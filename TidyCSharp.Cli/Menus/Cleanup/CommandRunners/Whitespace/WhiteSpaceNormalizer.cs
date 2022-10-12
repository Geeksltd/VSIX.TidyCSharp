using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace.Option;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace;

public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        return await NormalizeWhiteSpaceHelperAsync(initialSourceNode, Options);
    }

    public Options Options { get; set; }

    public async Task<SyntaxNode> NormalizeWhiteSpaceHelperAsync(SyntaxNode initialSourceNode, Options options)
    {
        var modifiedSourceNode = initialSourceNode;

        if (TidyCSharpPackage.Instance != null)
        {
            modifiedSourceNode = Formatter.Format(modifiedSourceNode, TidyCSharpPackage.Instance.Solution.Workspace);
        }

        var blockRewriter = new BlockRewriter(modifiedSourceNode, IsReportOnlyMode, options);
        modifiedSourceNode = blockRewriter.Visit(modifiedSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(blockRewriter.GetReport());
        }

        modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

        if (CheckOption((int)CleanupTypes.UseSlashInsteadOfSlashSlash))
        {
            var endoflineRewriter = new EndOfLineRewriter(modifiedSourceNode, IsReportOnlyMode, options);
            modifiedSourceNode = endoflineRewriter.Visit(modifiedSourceNode);
            modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(endoflineRewriter.GetReport());
            }
        }

        var whitespaceRewriter = new WhiteSpaceRewriter(modifiedSourceNode, IsReportOnlyMode, options);
        modifiedSourceNode = whitespaceRewriter.Apply();

        if (IsReportOnlyMode)
        {
            CollectMessages(whitespaceRewriter.GetReport());
        }

        if (CheckOption((int)CleanupTypes.AddBlankLineBetweenStatementsMoreThanOneLine))
        {
            modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);
            var blRewriter = new BlankLineRewriter(modifiedSourceNode, IsReportOnlyMode, ProjectItemDetails.SemanticModel);
            modifiedSourceNode = blRewriter.Visit(modifiedSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(blRewriter.GetReport());
            }
        }

        if (IsReportOnlyMode) return initialSourceNode;
        return modifiedSourceNode;
    }
}