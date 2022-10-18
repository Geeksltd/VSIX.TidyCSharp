using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Extensions;

public static class FileExtensions
{
    public static bool IsCsharpFile(this Document projectItem)
    {
        return projectItem.SourceCodeKind == SourceCodeKind.Regular && projectItem.Name.EndsWith(".cs");
    }

    public static bool IsCSharpDesignerFile(this Document projectItem)
    {
        return projectItem.SourceCodeKind == SourceCodeKind.Regular
               && projectItem.Name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
    }
}