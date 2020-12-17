using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
