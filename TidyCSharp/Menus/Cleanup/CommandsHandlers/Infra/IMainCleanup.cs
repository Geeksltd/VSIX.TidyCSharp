using Geeks.VSIX.TidyCSharp.Cleanup;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public interface IMainCleanup
    {
        bool IsMainObjectSelected { get; }
        CodeCleanerType MainCleanupItemType { get; }
        bool HasSubItems { get; }
        CheckBoxItemInfo[] GetSelectedSubItems();
        void SetSubItems(int value);
        void SetMainItemSelection(bool isSelected);
        void ReSetSubItems(bool selectAll = false);
    }
}
