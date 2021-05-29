using Geeks.VSIX.TidyCSharp.Cleanup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public class ReadOnlyMainCleanup : IMainCleanup
    {
        public bool IsMainObjectSelected { get; private set; }
        public CodeCleanerType MainCleanupItemType { get; private set; }
        public bool HasSubItems { get; private set; }

        public ReadOnlyMainCleanup(CodeCleanerType mainCleanupItemType, bool hasSubItems = false,
            bool isMainObjectSelected = true)
        {
            MainCleanupItemType = mainCleanupItemType;
            HasSubItems = hasSubItems;
            IsMainObjectSelected = isMainObjectSelected;
        }

        public CleanerItemUIInfo[] GetSelectedSubItems()
        {
            throw new NotImplementedException();
        }

        public void ResetItemsCheckState()
        {
            throw new NotImplementedException();
        }

        public void SetItemsCheckState(int value, bool checkedState)
        {
            throw new NotImplementedException();
        }

        public void SetMainItemCheckState(bool isSelected)
        {
            IsMainObjectSelected = isSelected;
        }
    }
}
