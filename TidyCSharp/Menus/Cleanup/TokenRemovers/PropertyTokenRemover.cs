using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class PropertyTokenRemover : IPrivateModiferTokenRemover
    {
        public SyntaxNode Remove(SyntaxNode root)
        {
            var properties = new PropertyExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);
            // TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
            return properties.Count == 0 ? null : root.RemovePrivateTokens(properties);
        }
    }
}