using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

internal interface ISyntaxWrapper<T>
    where T : SyntaxNode
{
    /// <summary>
    /// Gets the wrapped syntax node.
    /// </summary>
    /// <value>
    /// The wrapped syntax node.
    /// </value>
    T SyntaxNode
    {
        get;
    }
}