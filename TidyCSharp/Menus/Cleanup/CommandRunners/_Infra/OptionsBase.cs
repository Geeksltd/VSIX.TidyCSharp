using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public abstract class OptionsBase : ICleanupOption
    {
        public virtual int? CleanupItemsInteger { get; set; }

        protected abstract CodeCleanerType GetCodeCleanerType();


        public void Accept(IMainCleanup mainCleanup)
        {
            if (mainCleanup.MainCleanupItemType == GetCodeCleanerType())
            {
                var selectedItems = mainCleanup.GetSubItems().Select(x => x.CleanerType).ToArray();

                CleanupItemsInteger = selectedItems.FirstOrDefault();

                foreach (var item in selectedItems)
                {
                    CleanupItemsInteger |= item;
                }
            }
        }
    }

}
