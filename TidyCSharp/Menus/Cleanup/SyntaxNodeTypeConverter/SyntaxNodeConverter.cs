using Microsoft.CodeAnalysis;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter
{
    public static class SyntaxNodeConverter
    {
        public static T As<T>(this SyntaxNode node) where T : class
=> node as T;
    }
}
