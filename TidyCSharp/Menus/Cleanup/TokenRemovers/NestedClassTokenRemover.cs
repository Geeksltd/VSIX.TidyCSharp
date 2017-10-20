using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class NestedClassTokenRemover : IPrivateModiferTokenRemover
    {
        public SyntaxNode Remove(SyntaxNode root)
        {
            var nestedClasses = new NestedClassExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);
            // TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
            return nestedClasses.Count == 0 ? null : root.RemovePrivateTokens(nestedClasses);
        }
    }
}
