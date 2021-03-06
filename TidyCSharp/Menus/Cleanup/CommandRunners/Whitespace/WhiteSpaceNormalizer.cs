using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return NormalizeWhiteSpaceHelper(initialSourceNode, Options);
        }

        public Options Options { get; set; }

        public SyntaxNode NormalizeWhiteSpaceHelper(SyntaxNode initialSourceNode, Options options)
        {
            if (TidyCSharpPackage.Instance != null)
            {
                initialSourceNode = Formatter.Format(initialSourceNode, TidyCSharpPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            initialSourceNode = new BlockRewriter(initialSourceNode, options).Visit(initialSourceNode);
            initialSourceNode = RefreshResult(initialSourceNode);

            if (CheckOption((int)CleanupTypes.Use_slash_n_instead_of_slash_sr_slash_n))
            {
                initialSourceNode = new EndOFLineRewriter().Visit(initialSourceNode);
                initialSourceNode = RefreshResult(initialSourceNode);
            }

            initialSourceNode = new WhitespaceRewriter(initialSourceNode, options).Apply();
            if (CheckOption((int)CleanupTypes.Add_blank_line_between_statements_more_than_one_line))
            {
                initialSourceNode = RefreshResult(initialSourceNode);
                initialSourceNode = new BlankLineRewriter(this.ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            }
            return initialSourceNode;
        }
    }
}