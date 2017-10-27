using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace;
using Geeks.GeeksProductivityTools;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class WhiteSpaceNormalizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return NormalizeWhiteSpaceHelper(initialSourceNode, Options);
        }

        WhiteSpaceNormalizerOptions Options { get; set; }

        public static SyntaxNode NormalizeWhiteSpaceHelper(SyntaxNode initialSourceNode, WhiteSpaceNormalizerOptions options)
        {
            if (GeeksProductivityToolsPackage.Instance != null)
            {
                initialSourceNode = Formatter.Format(initialSourceNode, GeeksProductivityToolsPackage.Instance.CleanupWorkingSolution.Workspace);
            }

            initialSourceNode = new BlockRewriter(initialSourceNode, options).Visit(initialSourceNode);
            initialSourceNode = new WhitespaceRewriter(initialSourceNode, options).Apply();
            return initialSourceNode;
        }
    }
}