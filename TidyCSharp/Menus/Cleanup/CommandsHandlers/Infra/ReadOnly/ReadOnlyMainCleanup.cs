using Geeks.VSIX.TidyCSharp.Cleanup;
using System;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public class ReadOnlyMainCleanup : IMainCleanup
    {
        public bool IsMainObjectSelected { get; private set; }
        public CodeCleanerType MainCleanupItemType { get; private set; }
        public bool HasSubitems { get; private set; }
        public CleanerItemUIInfo[] CleanerItemUIInfos { get; private set; }

        public ReadOnlyMainCleanup(CodeCleanerType mainCleanupItemType,
            bool isMainObjectSelected = true)
        {
            MainCleanupItemType = mainCleanupItemType;
            IsMainObjectSelected = isMainObjectSelected;
        }

        public ReadOnlyMainCleanup(CodeCleanerType mainCleanupItemType, CleanerItemUIInfo[] cleanerItemUIInfos,
            bool isMainObjectSelected = true)
        {
            MainCleanupItemType = mainCleanupItemType;
            CleanerItemUIInfos = cleanerItemUIInfos;
            IsMainObjectSelected = isMainObjectSelected;
        }

        public CleanerItemUIInfo[] GetSelectedSubitems() => CleanerItemUIInfos;

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
}
