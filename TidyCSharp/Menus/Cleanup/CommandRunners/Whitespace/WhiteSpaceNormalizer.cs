using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhiteSpace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
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
                modifiedSourceNode = Formatter.Format(modifiedSourceNode, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            var blockRewriter = new BlockRewriter(modifiedSourceNode, IsReportOnlyMode, options);
            modifiedSourceNode = blockRewriter.Visit(modifiedSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(blockRewriter.GetReport());
            }

            modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

            if (CheckOption((int)CleanupTypes.Use_Slash_Instead_Of_Slash_Slash))
            {
                var endoflineRewriter = new EndOFLineRewriter(modifiedSourceNode, IsReportOnlyMode, options);
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

            if (CheckOption((int)CleanupTypes.Add_Blank_Line_Between_Statements_More_Than_One_Line))
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
}