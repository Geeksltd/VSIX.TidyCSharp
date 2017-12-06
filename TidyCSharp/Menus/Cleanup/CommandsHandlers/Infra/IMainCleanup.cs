using Geeks.VSIX.TidyCSharp.Cleanup;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public interface IMainCleanup
    {
        bool IsMainObjectSelected { get; }
        CodeCleanerType MainCleanupItemType { get; }
        bool HasSubItems { get; }
        CleanerItemUIInfo[] GetSelectedSubItems();
        void SetItemsCheckState(int value, bool checkedState);
        void SetMainItemCheckState(bool isSelected);
        void ResetItemsCheckState();
    }
}
