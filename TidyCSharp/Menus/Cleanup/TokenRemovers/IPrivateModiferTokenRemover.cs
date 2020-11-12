using Microsoft.CodeAnalysis;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public interface IPrivateModiferTokenRemover
    {
        SyntaxNode Remove(SyntaxNode root);
    }
}