using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers
{
    public class CleanupOptions
    {
        List<ICleanupOption> optionItems = new List<ICleanupOption>();

        public const string To_String_Seprator2 = ":", To_String_Seprator = ";";
        public List<CodeCleanerType> ActionTypes { get; private set; } = new List<CodeCleanerType>();

        public CleanupOptions()
        {
            optionItems.Add(CamelCasedFields = new CamelCasedClassFields.Options());
            optionItems.Add(CamelCasedLocalVariable = new CamelCasedMethodVariable.Options());
            optionItems.Add(ConvertMembersToExpressionBodied = new MembersToExpressionBodied.Options());
            optionItems.Add(PrivateModifierRemover = new RemovePrivateModifier.Options());
            optionItems.Add(RemoveExtraThisQualification = new RemoveExtraThisKeyword.Options());
            optionItems.Add(SimplifyClassFieldDeclarations = new SimplifyClassFieldDeclaration.Options());
            optionItems.Add(SimplyAsyncCall = new SimplyAsyncCall.Options());
            optionItems.Add(WhiteSpaceNormalizer = new NormalizeWhitespace.Options());
        }

        public void Accept(IMainCleanup mainCleanup)
        {
            foreach (var item in optionItems)
                item.Accept(mainCleanup);

            if (mainCleanup.IsMainObjectSelected)
            {
                ActionTypes.Add(mainCleanup.MainCleanupItemType);
            }
        }

        public CamelCasedClassFields.Options CamelCasedFields { get; private set; }
        public CamelCasedMethodVariable.Options CamelCasedLocalVariable { get; private set; }
        public MembersToExpressionBodied.Options ConvertMembersToExpressionBodied { get; private set; }
        public RemovePrivateModifier.Options PrivateModifierRemover { get; private set; }
        public RemoveExtraThisKeyword.Options RemoveExtraThisQualification { get; private set; }
        public SimplifyClassFieldDeclaration.Options SimplifyClassFieldDeclarations { get; private set; }
        public SimplyAsyncCall.Options SimplyAsyncCall { get; private set; }
        public NormalizeWhitespace.Options WhiteSpaceNormalizer { get; private set; }

        public string SerializeValues()
        {
            var values = System.Enum.GetValues(typeof(CodeCleanerType)).Cast<int>();

            return string.Join(To_String_Seprator, values
                .Select(enumValueAsObject =>
                {
                    var optionItem = optionItems.FirstOrDefault(o => (int)o.GetCodeCleanerType() == enumValueAsObject);

                    var IsParentSelected = ActionTypes.Contains((CodeCleanerType)enumValueAsObject);

                    if (optionItem == null) return $"{enumValueAsObject}{To_String_Seprator2}{IsParentSelected}{To_String_Seprator2}{-1}";

                    if (optionItem.CleanupItemsInteger.HasValue == false)
                        return $"{enumValueAsObject}{To_String_Seprator2}{IsParentSelected}{To_String_Seprator2}{0}";

                    return $"{enumValueAsObject}{To_String_Seprator2}{IsParentSelected}{To_String_Seprator2}{optionItem.CleanupItemsInteger.Value}";
                })
            );
        }

        public override string ToString()
        {
            return string.Join(To_String_Seprator, ActionTypes
                .Select(x =>
                {
                    var optionItem = optionItems.FirstOrDefault(o => o.GetCodeCleanerType() == x);
                    if (optionItem == null) return $"{(int)x}{To_String_Seprator2}{-1}";
                    if (optionItem.CleanupItemsInteger.HasValue == false) return $"{(int)x}{To_String_Seprator2}{0}";
                    return $"{(int)x}{To_String_Seprator2}{optionItem.CleanupItemsInteger.Value}";
                })
            );
        }
    }
}