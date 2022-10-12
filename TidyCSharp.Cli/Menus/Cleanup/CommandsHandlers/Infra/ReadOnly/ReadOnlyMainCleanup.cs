namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra.ReadOnly;

public class ReadOnlyMainCleanup : IMainCleanup
{
    public bool IsMainObjectSelected { get; private set; }
    public CodeCleanerType MainCleanupItemType { get; private set; }
    public bool HasSubitems { get; private set; }
    public CleanerItemUiInfo[] CleanerItemUiInfos { get; private set; }

    public ReadOnlyMainCleanup(CodeCleanerType mainCleanupItemType,
        bool isMainObjectSelected = true)
    {
        MainCleanupItemType = mainCleanupItemType;
        IsMainObjectSelected = isMainObjectSelected;
    }

    public ReadOnlyMainCleanup(CodeCleanerType mainCleanupItemType, CleanerItemUiInfo[] cleanerItemUiInfos,
        bool isMainObjectSelected = true)
    {
        MainCleanupItemType = mainCleanupItemType;
        CleanerItemUiInfos = cleanerItemUiInfos;
        IsMainObjectSelected = isMainObjectSelected;
    }

    public CleanerItemUiInfo[] GetSelectedSubitems() => CleanerItemUiInfos;

    public void ResetItemsCheckState()
    {
        throw new NotImplementedException();
    }

    public void SetItemsCheckState(int value, bool checkedState)
    {
        throw new NotImplementedException();
    }

    public void SetMainItemCheckState(bool isSelected) => IsMainObjectSelected = isSelected;
}