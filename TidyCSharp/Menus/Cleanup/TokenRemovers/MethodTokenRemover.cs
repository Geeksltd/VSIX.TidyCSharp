using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class MethodTokenRemover : IPrivateModiferTokenRemover
    {
        public SyntaxNode Remove(SyntaxNode root)
        {
            var methods = new MethodExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);

            // TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
            return methods.Count == 0 ? null : root.RemovePrivateTokens(methods);
        }
    }
}
