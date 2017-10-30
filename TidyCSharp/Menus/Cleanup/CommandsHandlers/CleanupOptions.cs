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
            optionItems.Add(ConvertMembersToExpressionBodied = new MembersToExpressionBodied.Options());
            optionItems.Add(RemoveExtraThisQualification = new RemoveExtraThisKeyword.Options());
            optionItems.Add(SimplifyClassFieldDeclarations = new SimplifyClassFieldDeclaration.Options());
        }

        public void Accept(IMainCleanup mainCleanup)
        {
            foreach (var item in optionItems)
            {
                item.Accept(mainCleanup);
            }
            if (mainCleanup.GetSubItems().Any())
            {
                ActionTypes.Add(mainCleanup.MainCleanupItemType);
            }
        }

        public RemovePrivateModifier.Options PrivateModifierRemover { get; private set; }
        public NormalizeWhitespace.Options WhiteSpaceNormalizer { get; private set; }
        public MembersToExpressionBodied.Options ConvertMembersToExpressionBodied { get; private set; }
        public RemoveExtraThisKeyword.Options RemoveExtraThisQualification { get; private set; }
        public SimplifyClassFieldDeclaration.Options SimplifyClassFieldDeclarations { get; private set; }
        public List<CodeCleanerType> ActionTypes { get; private set; } = new List<CodeCleanerType>();
    }
}
