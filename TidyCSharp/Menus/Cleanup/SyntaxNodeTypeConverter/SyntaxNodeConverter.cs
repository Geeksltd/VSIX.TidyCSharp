using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter
{
    public static class SyntaxNodeConverter
    {
        public static T As<T>(this SyntaxNode node) where T : class
        {
            return node as T;
        }
    }
}
