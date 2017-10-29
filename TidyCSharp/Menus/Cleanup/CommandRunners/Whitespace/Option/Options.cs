using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    public class Options : ICleanupOption
    {
        public const int BLOCK_SINGLE_STATEMENT_MAX_LENGTH = 70;

        public CleanupTypes? CleanupItems { get; set; }

        public void Accept(IMainCleanup mainCleanup)
        {
            if (mainCleanup.MainCleanupItemType == CodeCleanerType.NormalizeWhiteSpaces)
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
