namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

public interface IMainCleanup
{
    bool IsMainObjectSelected { get; }
    CodeCleanerType MainCleanupItemType { get; }
    bool HasSubitems { get; }
    CleanerItemUiInfo[] GetSelectedSubitems();
    void SetItemsCheckState(int value, bool checkedState);
    void SetMainItemCheckState(bool isSelected);
    void ResetItemsCheckState();
}