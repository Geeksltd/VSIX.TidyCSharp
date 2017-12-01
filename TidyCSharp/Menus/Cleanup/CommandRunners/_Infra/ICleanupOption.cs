using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public interface ICleanupOption
    {
        int? CleanupItemsInteger { get; }

        void Accept(IMainCleanup mainCleanup);

        CodeCleanerType GetCodeCleanerType();
    }
    public static class ICleanupOptionHelper
    {
        public static bool Should(this ICleanupOption options, int? optionItem)
        {
            if (options == null) return true;
            if (options.CleanupItemsInteger == null) return true;
            if (optionItem == null) return true;

            return (options.CleanupItemsInteger & optionItem) == optionItem;
        }

    }
}
