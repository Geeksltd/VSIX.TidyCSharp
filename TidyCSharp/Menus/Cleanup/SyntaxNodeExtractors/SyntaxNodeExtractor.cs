using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
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
    }
}
