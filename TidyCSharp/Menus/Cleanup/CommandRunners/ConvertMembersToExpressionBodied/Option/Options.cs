using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied
{
    public class Options : ICleanupOption
    {
        public const int MAX_EXPRESSION_BODIED_MEMBER_LENGTH = 90;

        public CleanupTypes? CleanupItems { get; private set; }

        public int? CleanupItemsInteger => (int?)CleanupItems;

        //int? ICleanupOption.CleanupItems => (int?)CleanupItems;

        public void Accept(IMainCleanup mainCleanup)
        {
            if (mainCleanup.MainCleanupItemType == CodeCleanerType.ConvertMembersToExpressionBodied)
            {
                var selectedItems = mainCleanup.GetSubItems().Select(x => (CleanupTypes)x.CleanerType).ToArray();

                CleanupItems = selectedItems.FirstOrDefault();

                foreach (var item in selectedItems)
                {
                    CleanupItems |= item;
                }
            }
        }
    }

}
