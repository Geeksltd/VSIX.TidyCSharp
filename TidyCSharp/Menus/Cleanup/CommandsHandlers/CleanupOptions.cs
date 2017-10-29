using Geeks.GeeksProductivityTools.Definition;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers
{
    public class CleanupOptions
    {
        List<ICleanupOption> optionItems = new List<ICleanupOption>();

        public CleanupOptions()
        {
            optionItems.Add(PrivateModifierRemover = new RemovePrivateModifier.Options());
            optionItems.Add(WhiteSpaceNormalizer = new NormalizeWhitespace.Options());
        }

        public void Accept(IMainCleanup mainCleanup)
        {
            var temp = new List<CodeCleanerType>();

            foreach (var item in optionItems)
            {
                item.Accept(mainCleanup);
            }
            if (mainCleanup.GetSubItems().Any())
            {
                temp.Add(mainCleanup.MainCleanupItemType);
            }
            ActionTypes = temp.ToArray();
        }

        public RemovePrivateModifier.Options PrivateModifierRemover { get; private set; }
        public NormalizeWhitespace.Options WhiteSpaceNormalizer { get; private set; }
        public CodeCleanerType[] ActionTypes { get; private set; }
    }
}
