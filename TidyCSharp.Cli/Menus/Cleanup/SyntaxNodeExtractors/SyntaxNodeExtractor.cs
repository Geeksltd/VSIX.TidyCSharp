using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;

public static class SyntaxNodeExtractor
{
    public static IEnumerable<T> DescendantNodesOfType<T>(this SyntaxNode node) where T : class
    {
        return node.DescendantNodes().OfType<T>();
    }

    public static IEnumerable<T> DescendantNodesAndSelfOfType<T>(this SyntaxNode node) where T : class
    {
        return node.DescendantNodesAndSelf().OfType<T>();
    }

    public static T FirstDescendantNode<T>(this SyntaxNode node) where T : class
    {
        return node.DescendantNodes().OfType<T>().FirstOrDefault();
    }

    public static FileLinePositionSpan GetFileLinePosSpan(this SyntaxNode node)
    {
        return node.SyntaxTree.GetLineSpan(new TextSpan(node.Span.Start, node.Span.Length));
    }

    public static string GetFileName(this SyntaxNode node)
    {
        return Path.GetFileName(node.SyntaxTree.FilePath);
    }

    public static string GetFilePath(this SyntaxNode node) => node.SyntaxTree.FilePath;
}