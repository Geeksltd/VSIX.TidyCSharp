namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public static class CleanupOptionHelper
{
    public static bool Should(this ICleanupOption options, int? optionItem)
    {
        if (options == null) return true;
        if (options.CleanupItemsInteger == null) return true;
        if (optionItem == null) return true;

        return (options.CleanupItemsInteger & optionItem) == optionItem;
    }
}