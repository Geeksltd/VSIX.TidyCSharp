using Geeks.VSIX.TidyCSharp.Cleanup;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public interface IMainCleanup
    {
        CodeCleanerType MainCleanupItemType { get; set; }
        CheckBoxItem[] GetSubItems();
    }
}
