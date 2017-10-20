using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class FieldTokenRemover : IPrivateModiferTokenRemover
    {
        public SyntaxNode Remove(SyntaxNode root)
        {
            var fields = new FieldExtractor().Extraxt(root, SyntaxKind.PrivateKeyword);
            // TODO: 1. Fix the issue with touching the namespaces 2.Remove the conditional operator 
            return fields.Count == 0 ? null : root.RemovePrivateTokens(fields);
        }
    }
}
