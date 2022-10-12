using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeTypeConverter;

public static class SyntaxNodeConverter
{
    public static T As<T>(this SyntaxNode node) where T : class
        => node as T;
}