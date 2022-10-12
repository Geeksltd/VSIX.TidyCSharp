using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.TokenRemovers;

public interface IPrivateModiferTokenRemover
{
    SyntaxNode Remove(SyntaxNode root);
}