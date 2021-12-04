using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            return await NormalizeWhiteSpaceHelper(initialSourceNode, Options);
        }

        public Options Options { get; set; }

        public async Task<SyntaxNode> NormalizeWhiteSpaceHelper(SyntaxNode initialSourceNode, Options options)
        {
            var modifiedSourceNode = initialSourceNode;

            if (TidyCSharpPackage.Instance != null)
            {
                modifiedSourceNode = Formatter.Format(modifiedSourceNode, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            var blockRewriter = new BlockRewriter(modifiedSourceNode, IsReportOnlyMode, options);
            modifiedSourceNode = blockRewriter.Visit(modifiedSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(blockRewriter.GetReport());
            }

            modifiedSourceNode = await RefreshResult(modifiedSourceNode);

            if (CheckOption((int)CleanupTypes.Use_slash_n_instead_of_slash_sr_slash_n))
            {
                var endoflineRewriter = new EndOFLineRewriter(modifiedSourceNode, IsReportOnlyMode, options);
                modifiedSourceNode = endoflineRewriter.Visit(modifiedSourceNode);
                modifiedSourceNode = await RefreshResult(modifiedSourceNode);

                if (IsReportOnlyMode)
                {
                    CollectMessages(endoflineRewriter.GetReport());
                }
            }

            var whitespaceRewriter = new WhitespaceRewriter(modifiedSourceNode, IsReportOnlyMode, options);
            modifiedSourceNode = whitespaceRewriter.Apply();

            if (IsReportOnlyMode)
            {
                CollectMessages(whitespaceRewriter.GetReport());
            }

            if (CheckOption((int)CleanupTypes.Add_blank_line_between_statements_more_than_one_line))
            {
                modifiedSourceNode = await RefreshResult(modifiedSourceNode);
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
}