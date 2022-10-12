using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils;

public class ActiveDocument
{
    public static bool IsValid(Document item)
    {
        if (item == null) return false;

        var path = item.FilePath;

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return false;

        return true;
    }
}