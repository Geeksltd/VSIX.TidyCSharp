using System.Linq;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public abstract class OptionsBase : ICleanupOption
    {
        public virtual int? CleanupItemsInteger { get; private set; }

        public abstract CodeCleanerType GetCodeCleanerType();

        public void Accept(IMainCleanup mainCleanup)
        {
            if (mainCleanup.MainCleanupItemType == GetCodeCleanerType())
            {
                var selectedItems = mainCleanup.GetSelectedSubItems().Select(x => x.CleanerType).ToArray();

                CleanupItemsInteger = selectedItems.FirstOrDefault();

                foreach (var item in selectedItems)
                {
                    CleanupItemsInteger |= item;
                }
            }
        }

        public override string ToString()
        {
            return $"{(CleanupItemsInteger ?? 0).ToString()}";
            //return $"{(int)GetCodeCleanerType()}:{(CleanupItemsInteger ?? 0).ToString()}";
        }
    }

}
