namespace Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming
{
    using Microsoft.CodeAnalysis;

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
}