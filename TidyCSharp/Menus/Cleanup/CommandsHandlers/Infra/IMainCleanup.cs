using Geeks.VSIX.TidyCSharp.Cleanup;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public interface IMainCleanup
    {
        bool IsMainObjectSelected();
        CodeCleanerType MainCleanupItemType { get; set; }
        CheckBoxItemInfo[] GetSelectedSubItems();
        CheckBoxItemInfo[] GetSubItems();
    }
}
